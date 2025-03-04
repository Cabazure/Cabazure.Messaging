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
            .ReceiveMessagesAsync(default)
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

        _ = queueClient
            .Received(2)
            .ReceiveMessagesAsync(
                cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Call_Processor_For_Each_Message()
    {
        await sut.StartAsync(cancellationToken);

        _ = processor
            .Received(3)
            .ProcessAsync(
                Arg.Any<TMessage>(),
                Arg.Any<StorageQueueMetadata>(),
                Arg.Any<CancellationToken>());

        processor
            .ReceivedCallsWithArguments<TMessage>()
            .Should()
            .BeEquivalentTo(messages);

        processor
            .ReceivedCallsWithArguments<StorageQueueMetadata>()
            .Should()
            .BeEquivalentTo(queueMessages
                .Select(StorageQueueMetadata.Create));
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Call_Processor_ProcessErrorAsync(
        Exception exception)
    {
        processor.ProcessAsync(default, default, default)
            .ReturnsForAnyArgs(c => throw exception);

        await sut.StartAsync(cancellationToken);

        _ = processor
            .Received(3)
            .ProcessErrorAsync(
                exception,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Delete_Processed_Messages()
    {
        await sut.StartAsync(cancellationToken);

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

        timeProvider
            .Received(1)
            .CreateTimer(
                Arg.Any<TimerCallback>(),
                Arg.Any<object?>(),
                options.PollingInterval,
                Timeout.InfiniteTimeSpan);
    }
}
