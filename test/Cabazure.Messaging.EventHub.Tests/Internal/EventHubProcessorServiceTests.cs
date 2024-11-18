using System.Text.Json;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubProcessorServiceTests
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

    [Theory, AutoNSubstituteData]
    public async Task StartAsync_Starts_Processing(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IEventHubProcessor client,
        EventHubProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);

        _ = client
            .Received(1)
            .StartProcessingAsync(cancellationToken);
    }

    [Theory]
    [InlineAutoNSubstituteData(true)]
    [InlineAutoNSubstituteData(false)]
    public void IsRunning_Returns_From_Client(
        bool isRunning,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen] IEventHubProcessor client,
        EventHubProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        client.IsRunning.Returns(isRunning);

        sut.IsRunning.Should().Be(isRunning);
    }

    [Theory, AutoNSubstituteData]
    public async Task StopAsync_Stops_Processing(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IEventHubProcessor client,
        EventHubProcessorService<TMessage, TProcessor> sut,
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
        [Frozen, Substitute] IEventHubProcessor client,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubProcessorService<TMessage, TProcessor> sut,
        [Frozen, Substitute] PartitionContext partitionContext,
        ProcessEventArgs args,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        var json = JsonSerializer.Serialize(message, serializerOptions);
        args.Data.EventBody = new BinaryData(json);

        await sut.StartAsync(cancellationToken);
        client.ProcessEventAsync += Raise.Event<Func<ProcessEventArgs, Task>>(args);

        _ = processor
            .Received(1)
            .ProcessAsync(
                message,
                Arg.Any<EventHubMetadata>(),
                args.CancellationToken);

        processor
            .ReceivedCallWithArgument<EventHubMetadata>()
            .Should()
            .BeEquivalentTo(
                EventHubMetadata.Create(args.Data));
    }

    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Not_Called_When_Filter_Does_Not_Match(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IEventHubProcessor client,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubProcessorService<TMessage, TProcessor> sut,
        [Frozen, Substitute] PartitionContext partitionContext,
        ProcessEventArgs args,
        string propertyKey,
        string propertyValue,
        string filterValue,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        args.Data.Properties.Add(propertyKey, propertyValue);
        filters.Add(d => (string)d[propertyKey] == filterValue);

        var json = JsonSerializer.Serialize(message, serializerOptions);
        args.Data.EventBody = new BinaryData(json);

        await sut.StartAsync(cancellationToken);
        client.ProcessEventAsync += Raise.Event<Func<ProcessEventArgs, Task>>(args);

        _ = processor
            .DidNotReceiveWithAnyArgs()
            .ProcessAsync(default, default, default);
    }


    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Called_When_Filter_Does_Match(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IEventHubProcessor client,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubProcessorService<TMessage, TProcessor> sut,
        [Frozen, Substitute] PartitionContext partitionContext,
        ProcessEventArgs args,
        string propertyKey,
        string propertyValue,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        args.Data.Properties.Add(propertyKey, propertyValue);
        filters.Add(d => (string)d[propertyKey] == propertyValue);

        var json = JsonSerializer.Serialize(message, serializerOptions);
        args.Data.EventBody = new BinaryData(json);

        await sut.StartAsync(cancellationToken);
        client.ProcessEventAsync += Raise.Event<Func<ProcessEventArgs, Task>>(args);

        _ = processor
            .Received(1)
            .ProcessAsync(
                message,
                Arg.Any<EventHubMetadata>(),
                args.CancellationToken);

        processor
            .ReceivedCallWithArgument<EventHubMetadata>()
            .Should()
            .BeEquivalentTo(
                EventHubMetadata.Create(args.Data));
    }

    [Theory, AutoNSubstituteData]
    public async Task Processor_Is_Not_Called_When_Processor_Is_Stopped(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IEventHubProcessor client,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubProcessorService<TMessage, TProcessor> sut,
        [Frozen, Substitute] PartitionContext partitionContext,
        ProcessEventArgs args,
        TMessage message,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        var json = JsonSerializer.Serialize(message, serializerOptions);
        args.Data.EventBody = new BinaryData(json);

        await sut.StartAsync(cancellationToken);
        await sut.StopAsync(cancellationToken);
        client.ProcessEventAsync += Raise.Event<Func<ProcessEventArgs, Task>>(args);

        _ = processor
            .DidNotReceiveWithAnyArgs()
            .ProcessAsync(default, default, default);
    }

    [Theory, AutoNSubstituteData]
    public async Task Logger_Is_Called_When_Processing_Error_Occurs(
        [Frozen] ILogger<TProcessor> logger,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, Substitute] IEventHubProcessor client,
        EventHubProcessorService<TMessage, TProcessor> sut,
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
        [Frozen, Substitute] IEventHubProcessor client,
        EventHubProcessorService<TMessage, TProcessorWithErrorHandling> sut,
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
        [Frozen, Substitute] IEventHubProcessor client,
        EventHubProcessorService<TMessage, TProcessorWithErrorHandling> sut,
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
