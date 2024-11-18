using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.ServiceBus.Tests.Internal;

public class ServiceBusProcessorServiceTests
{
    public record TMessage(string Data);
    public class TProcessor : IMessageProcessor<TMessage>
    {
        public virtual Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
    public class TProcessorWithErrorHandling : IMessageProcessor<TMessage>, IProcessErrorHandler
    {
        public virtual Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual Task ProcessErrorAsync(Exception exception, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private static ProcessMessageEventArgs CreateProcessMessageEventArgs(
        string json,
        Dictionary<string, object>? properties = null)
    {
        var fixture = FixtureFactory.Create();
        var message = ServiceBusModelFactory
            .ServiceBusReceivedMessage(
                body: new BinaryData(json),
                messageId: fixture.Create<string>(),
                partitionKey: fixture.Create<string>(),
                viaPartitionKey: fixture.Create<string>(),
                sessionId: fixture.Create<string>(),
                replyToSessionId: fixture.Create<string>(),
                timeToLive: TimeSpan.FromMilliseconds(fixture.Create<double>()),
                correlationId: fixture.Create<string>(),
                subject: fixture.Create<string>(),
                to: fixture.Create<string>(),
                contentType: fixture.Create<string>(),
                replyTo: fixture.Create<string>(),
                scheduledEnqueueTime: fixture.Create<DateTimeOffset>(),
                properties: properties ?? fixture.Create<IDictionary<string, object>>(),
                lockTokenGuid: fixture.Create<Guid>(),
                deliveryCount: fixture.Create<int>(),
                lockedUntil: DateTimeOffset.UtcNow.AddDays(1),
                sequenceNumber: fixture.Create<long>(),
                deadLetterSource: fixture.Create<string>(),
                enqueuedSequenceNumber: fixture.Create<long>(),
                enqueuedTime: fixture.Create<DateTimeOffset>(),
                serviceBusMessageState: fixture.Create<ServiceBusMessageState>());

        return new ProcessMessageEventArgs(
            message,
            Substitute.For<ServiceBusReceiver>(),
            fixture.Create<string>(),
            fixture.Create<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task StartAsync_Starts_Processing(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);

        _ = client
            .Received(1)
            .StartProcessingAsync(cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task IsRunning_Returns_True_When_Started(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen] IServiceBusProcessor client,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);
        client.IsProcessing.Returns(true);

        sut.IsRunning.Should().BeTrue();
    }

    [Theory, AutoNSubstituteData]
    public void IsRunning_Returns_False_When_Stopped(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen] IServiceBusProcessor client,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        client.IsProcessing.Returns(false);

        sut.IsRunning.Should().BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task StopAsync_Stops_Processing(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);
        await sut.StopAsync(cancellationToken);

        _ = client
            .Received(1)
            .StopProcessingAsync(cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Called_When_Client_Receives_Message(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        [Frozen] List<Func<IReadOnlyDictionary<string, object>, bool>> filters,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        var json = JsonSerializer.Serialize(message, serializerOptions);
        var args = CreateProcessMessageEventArgs(json);

        await sut.StartAsync(cancellationToken);
        client.ProcessMessageAsync += Raise.Event<Func<ProcessMessageEventArgs, Task>>(args);

        _ = processor
            .Received(1)
            .ProcessAsync(
                message,
                Arg.Any<ServiceBusMetadata>(),
                args.CancellationToken);

        processor
            .ReceivedCallWithArgument<ServiceBusMetadata>()
            .Should()
            .BeEquivalentTo(
                ServiceBusMetadata.Create(args.Message));
    }

    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Not_Called_When_Filter_Does_Not_Match(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        [Frozen] List<Func<IReadOnlyDictionary<string, object>, bool>> filters,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        string propertyKey,
        string propertyValue,
        string filterValue,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        filters.Add(d => (string)d[propertyKey] == filterValue);
        var json = JsonSerializer.Serialize(message, serializerOptions);
        var args = CreateProcessMessageEventArgs(
            json,
            new()
            {
                [propertyKey] = propertyValue,
            });

        await sut.StartAsync(cancellationToken);
        client.ProcessMessageAsync += Raise.Event<Func<ProcessMessageEventArgs, Task>>(args);

        _ = processor
            .DidNotReceiveWithAnyArgs()
            .ProcessAsync(default, default, default);
    }


    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Called_When_Filter_Does_Match(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        [Frozen] List<Func<IReadOnlyDictionary<string, object>, bool>> filters,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        string propertyKey,
        string propertyValue,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        filters.Add(d => (string)d[propertyKey] == propertyValue);
        var json = JsonSerializer.Serialize(message, serializerOptions);
        var args = CreateProcessMessageEventArgs(
            json,
            new()
            {
                [propertyKey] = propertyValue,
            });

        await sut.StartAsync(cancellationToken);
        client.ProcessMessageAsync += Raise.Event<Func<ProcessMessageEventArgs, Task>>(args);

        _ = processor
            .Received(1)
            .ProcessAsync(
                message,
                Arg.Any<ServiceBusMetadata>(),
                args.CancellationToken);

        processor
            .ReceivedCallWithArgument<ServiceBusMetadata>()
            .Should()
            .BeEquivalentTo(
                ServiceBusMetadata.Create(args.Message));
    }

    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Not_Called_When_Processor_Is_Stopped(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        [Frozen] List<Func<IReadOnlyDictionary<string, object>, bool>> filters,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        var json = JsonSerializer.Serialize(message, serializerOptions);
        var args = CreateProcessMessageEventArgs(json);

        await sut.StartAsync(cancellationToken);
        await sut.StopAsync(cancellationToken);
        client.ProcessMessageAsync += Raise.Event<Func<ProcessMessageEventArgs, Task>>(args);

        _ = processor
            .DidNotReceiveWithAnyArgs()
            .ProcessAsync(default, default, default);
    }

    [Theory, AutoNSubstituteData]
    public async Task Logger_Is_Called_When_Processing_Error_Occurs(
        [Frozen] ILogger<TProcessor> logger,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        ServiceBusProcessorService<TMessage, TProcessor> sut,
        ProcessErrorEventArgs args,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);
        client.ProcessErrorAsync += Raise.Event<Func<ProcessErrorEventArgs, Task>>(args);

        logger
            .Received(1)
            .FailedToProcessMessage(
                nameof(TMessage),
                nameof(TProcessor),
                args.Exception);
    }

    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Called_When_Processing_Error_Occurs(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessorWithErrorHandling processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        ServiceBusProcessorService<TMessage, TProcessorWithErrorHandling> sut,
        ProcessErrorEventArgs args,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);
        client.ProcessErrorAsync += Raise.Event<Func<ProcessErrorEventArgs, Task>>(args);

        _ = processor
            .Received(1)
            .ProcessErrorAsync(
                args.Exception,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Not_Called_When_Processing_Error_Occurs_And_Processor_Is_Stopped(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessorWithErrorHandling processor,
        [Frozen, Substitute] IServiceBusProcessor client,
        ServiceBusProcessorService<TMessage, TProcessorWithErrorHandling> sut,
        ProcessErrorEventArgs args,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);
        await sut.StopAsync(cancellationToken);
        client.ProcessErrorAsync += Raise.Event<Func<ProcessErrorEventArgs, Task>>(args);

        _ = processor
            .DidNotReceiveWithAnyArgs()
            .ProcessErrorAsync(default, default);
    }
}
