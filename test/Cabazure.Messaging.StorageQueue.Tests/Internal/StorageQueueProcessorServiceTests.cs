using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Cabazure.Messaging.StorageQueue.Internal;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.StorageQueue.Tests.Internal;

public sealed class StorageQueueProcessorServiceTests : IDisposable
{
    public record TMessage(string Data);
    public class TProcessor : IMessageProcessor<TMessage>, IProcessErrorHandler
    {
        public virtual Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual Task ProcessErrorAsync(Exception exception, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private readonly TimeProvider timeProvider;
    private readonly ILogger<TProcessor> logger;
    private readonly TProcessor processor;
    private readonly StorageQueueProcessorOptions options;
    private readonly JsonSerializerOptions serializerOptions;
    private readonly QueueClient queueClient;
    private readonly StorageQueueProcessorService<TMessage, TProcessor> sut;
    private readonly CancellationToken cancellationToken;
    private readonly TMessage[] messages;
    private readonly QueueMessage[] queueMessages;

    public void Dispose() => sut.Dispose();

    public StorageQueueProcessorServiceTests()
    {
        // Create substitute dependencies
        timeProvider = Substitute.For<TimeProvider>();
        logger = Substitute.For<ILogger<TProcessor>>();
        processor = Substitute.For<TProcessor>();
        options = new StorageQueueProcessorOptions();
        serializerOptions = new JsonSerializerOptions();
        queueClient = Substitute.For<QueueClient>();

        // Create the system under test
        sut = new StorageQueueProcessorService<TMessage, TProcessor>(
            timeProvider,
            logger,
            processor,
            options,
            serializerOptions,
            queueClient);


        // Cancel token when using timeProvider for delay
        var cts = new CancellationTokenSource();
        cancellationToken = cts.Token;
        timeProvider
            .CreateTimer(default, default, default, default)
            .ReturnsForAnyArgs(c =>
            {
                cts.Cancel();
                c.Arg<TimerCallback>().Invoke(c.Arg<object?>());
                return Substitute.For<ITimer>();
            });

        // Create sample queue messages
        var fixture = FixtureFactory.Create();
        messages = fixture.Create<TMessage[]>();
        queueMessages = messages
            .Select(m => QueuesModelFactory.QueueMessage(
                messageId: fixture.Create<string>(),
                popReceipt: fixture.Create<string>(),
                BinaryData.FromObjectAsJson(m, serializerOptions),
                dequeueCount: 0,
                nextVisibleOn: fixture.Create<DateTimeOffset>(),
                insertedOn: fixture.Create<DateTimeOffset>(),
                expiresOn: fixture.Create<DateTimeOffset>()))
            .ToArray();

        // Add a response with queue messages
        // and an empty response to trigger the delay and end the loop
        var response1 = Azure.Response.FromValue(queueMessages, Substitute.For<Azure.Response>());
        var response2 = Azure.Response.FromValue(new QueueMessage[0], Substitute.For<Azure.Response>());
        queueClient
            .ReceiveMessagesAsync(default, default, default)
            .ReturnsForAnyArgs(response1, response2);
    }

    [Fact]
    public void Has_Processor()
        => sut.Processor.Should().Be(processor);

    [Fact]
    public async Task Can_Start_And_Stop()
    {
        sut.IsRunning.Should().BeFalse();

        await sut.StartAsync(cancellationToken);

        sut.IsRunning.Should().BeTrue();

        await sut.StopAsync(cancellationToken);

        sut.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Create_Queue_If_CreateIfNotExists_Is_True()
    {
        options.CreateIfNotExists = true;

        await sut.StartAsync(cancellationToken);
        await queueClient.WaitForReceivedWithAnyArgs(c
            => c.CreateIfNotExistsAsync(default, TestContext.Current.CancellationToken));

        _ = queueClient
            .Received(1)
            .CreateIfNotExistsAsync(
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Create_Queue_If_CreateIfNotExists_Is_False()
    {
        options.CreateIfNotExists = false;

        await sut.StartAsync(cancellationToken);

        _ = queueClient
            .DidNotReceiveWithAnyArgs()
            .CreateIfNotExistsAsync(
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Receive_Messages_From_Queue()
    {
        await sut.StartAsync(cancellationToken);
        await sut.ExecuteTask!;

        _ = queueClient
            .Received(2)
            .ReceiveMessagesAsync(
                maxMessages: default,
                visibilityTimeout: default,
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Use_Configured_Message_Claim_Duration()
    {
        options.MessageClaimDuration = TimeSpan.FromMinutes(2);

        await sut.StartAsync(cancellationToken);
        await sut.ExecuteTask!;

        _ = queueClient
            .Received(2)
            .ReceiveMessagesAsync(
                maxMessages: default,
                visibilityTimeout: options.MessageClaimDuration,
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Call_Processor_For_Each_Message()
    {
        await sut.StartAsync(cancellationToken);
        await sut.ExecuteTask!;

        _ = processor
            .Received(3)
            .ProcessAsync(
                Arg.Any<TMessage>(),
                Arg.Any<StorageQueueMetadata>(),
                Arg.Any<CancellationToken>());

        processor
            .ReceivedArgs<TMessage>()
            .Should()
            .BeEquivalentTo(messages);

        processor
            .ReceivedArgs<StorageQueueMetadata>()
            .Should()
            .BeEquivalentTo(queueMessages
                .Select(StorageQueueMetadata.Create));
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Call_Processor_ProcessErrorAsync(
        Exception exception)
    {
        processor.ProcessAsync(default!, default!, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(c => throw exception);

        await sut.StartAsync(cancellationToken);
        await sut.ExecuteTask!;

        _ = processor
            .Received(3)
            .ProcessErrorAsync(
                exception,
                Arg.Any<CancellationToken>());

        foreach (var message in queueMessages)
        {
            _ = queueClient.Received(1).DeleteMessageAsync(
                message.MessageId,
                message.PopReceipt,
                Arg.Any<CancellationToken>());
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Retry_Failed_Message_And_Delete_It_Only_After_Success(
        bool operationCanceled)
    {
        options.RetryDelay = TimeSpan.FromMinutes(1);
        Exception exception = operationCanceled
            ? new OperationCanceledException()
            : new InvalidOperationException();
        processor
            .ProcessAsync(messages[0], Arg.Any<MessageMetadata>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw exception, _ => Task.CompletedTask);
        var original = queueMessages[0];
        var redelivered = QueuesModelFactory.QueueMessage(
            original.MessageId,
            original.PopReceipt + "-retry",
            original.Body,
            original.DequeueCount + 1,
            original.NextVisibleOn,
            original.InsertedOn,
            original.ExpiresOn);
        queueClient
            .ReceiveMessagesAsync(default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(
                Azure.Response.FromValue(queueMessages, Substitute.For<Azure.Response>()),
                Azure.Response.FromValue(new[] { redelivered }, Substitute.For<Azure.Response>()),
                Azure.Response.FromValue(Array.Empty<QueueMessage>(), Substitute.For<Azure.Response>()));

        await sut.InvokeProtected<Task>("ExecuteAsync", cancellationToken);

        _ = queueClient.Received(1).UpdateMessageAsync(
            original.MessageId,
            original.PopReceipt,
            visibilityTimeout: options.RetryDelay.Value,
            cancellationToken: Arg.Any<CancellationToken>());
        _ = queueClient.DidNotReceive().DeleteMessageAsync(
            original.MessageId,
            original.PopReceipt,
            Arg.Any<CancellationToken>());
        foreach (var message in queueMessages.Skip(1).Append(redelivered))
        {
            _ = queueClient.Received(1).DeleteMessageAsync(
                message.MessageId,
                message.PopReceipt,
                Arg.Any<CancellationToken>());
        }

        _ = processor.Received(2).ProcessAsync(
            messages[0],
            Arg.Any<StorageQueueMetadata>(),
            Arg.Any<CancellationToken>());
        _ = processor.Received(1).ProcessErrorAsync(
            exception,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Retry_Failed_Messages_Without_A_Custom_Error_Handler()
    {
        options.RetryDelay = TimeSpan.FromMinutes(1);
        var plainProcessor = Substitute.For<IMessageProcessor<TMessage>>();
        plainProcessor
            .ProcessAsync(default!, default!, TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());
        using var plainSut = new StorageQueueProcessorService<TMessage, IMessageProcessor<TMessage>>(
            timeProvider,
            Substitute.For<ILogger<IMessageProcessor<TMessage>>>(),
            plainProcessor,
            options,
            serializerOptions,
            queueClient);

        await plainSut.InvokeProtected<Task>("ExecuteAsync", cancellationToken);

        foreach (var message in queueMessages)
        {
            _ = queueClient.Received(1).UpdateMessageAsync(
                message.MessageId,
                message.PopReceipt,
                visibilityTimeout: options.RetryDelay.Value,
                cancellationToken: Arg.Any<CancellationToken>());
        }

        _ = queueClient.DidNotReceiveWithAnyArgs().DeleteMessageAsync(
            default!,
            default!,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Not_Delete_Message_When_Retry_Scheduling_Fails()
    {
        options.RetryDelay = TimeSpan.FromMinutes(1);
        processor
            .ProcessAsync(default!, default!, TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());
        var exception = new Azure.RequestFailedException("Queue unavailable");
        queueClient
            .UpdateMessageAsync(
                default!,
                default!,
                visibilityTimeout: default,
                cancellationToken: TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(exception);

        var act = () => sut.InvokeProtected<Task>("ExecuteAsync", cancellationToken);

        await act.Should().ThrowAsync<Azure.RequestFailedException>()
            .Where(e => ReferenceEquals(e, exception));
        _ = queueClient.DidNotReceiveWithAnyArgs().DeleteMessageAsync(
            default!,
            default!,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Leave_Message_Unsettled_When_Processing_Is_Canceled_For_Shutdown()
    {
        options.RetryDelay = TimeSpan.FromMinutes(1);
        using var cts = new CancellationTokenSource();
        processor
            .ProcessAsync(default!, default!, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(_ =>
            {
                cts.Cancel();
                throw new OperationCanceledException(cts.Token);
            });

        await sut.InvokeProtected<Task>("ExecuteAsync", cts.Token);

        _ = processor.DidNotReceiveWithAnyArgs().ProcessErrorAsync(
            default!,
            TestContext.Current.CancellationToken);
        _ = queueClient.DidNotReceiveWithAnyArgs().DeleteMessageAsync(
            default!,
            default!,
            TestContext.Current.CancellationToken);
        _ = queueClient.DidNotReceiveWithAnyArgs().UpdateMessageAsync(
            default!,
            default!,
            visibilityTimeout: default,
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Delete_Processed_Messages()
    {
        await sut.StartAsync(cancellationToken);
        await sut.ExecuteTask!;

        foreach (var message in queueMessages)
        {
            _ = queueClient
                .Received(1)
                .DeleteMessageAsync(
                    message.MessageId,
                    message.PopReceipt,
                    Arg.Any<CancellationToken>());

        }
    }

    [Fact]
    public async Task Should_Add_A_PollingInterval_Delay_When_No_Messages_Was_Received()
    {
        await sut.StartAsync(cancellationToken);
        await sut.ExecuteTask!;

        timeProvider
            .Received(1)
            .CreateTimer(
                Arg.Any<TimerCallback>(),
                Arg.Any<object?>(),
                options.PollingInterval,
                Timeout.InfiniteTimeSpan);
    }
}
