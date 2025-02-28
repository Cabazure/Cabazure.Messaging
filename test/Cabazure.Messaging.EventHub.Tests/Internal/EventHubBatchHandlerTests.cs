using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubBatchHandlerTests
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
    public async Task ProcessBatchAsync_Should_Call_Processor_For_Each_Message(
        [Frozen] ILogger<TProcessor> logger,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubBatchHandler<TMessage, TProcessor> sut,
        EventData[] data,
        TMessage[] messages,
        EventProcessorPartition partition,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        for (int i = 0; i < data.Length; i++)
        {
            data[i].EventBody = BinaryData.FromObjectAsJson(
                messages[i],
                serializerOptions);
        }

        await sut.ProcessBatchAsync(
            data,
            partition,
            cancellationToken);

        for (int i = 0; i < data.Length; i++)
        {
            _ = processor
                .Received(1)
                .ProcessAsync(
                    messages[i],
                    Arg.Any<EventHubMetadata>(),
                    cancellationToken);
        }

        var expectedMetadata = data
            .Select(e => EventHubMetadata.Create(e, partition.PartitionId))
            .ToArray();
        processor
            .ReceivedCallsWithArguments<EventHubMetadata>()
            .Should()
            .BeEquivalentTo(expectedMetadata);
    }

    [Theory, AutoNSubstituteData]
    public async Task ProcessBatchAsync_Should_Not_Call_Processor_When_Filter_Does_Not_Match(
        [Frozen] ILogger<TProcessor> logger,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubBatchHandler<TMessage, TProcessor> sut,
        EventData data,
        TMessage message,
        EventProcessorPartition partition,
        string propertyKey,
        string propertyValue,
        string filterValue,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        data.Properties.Add(propertyKey, propertyValue);
        filters.Add(d => (string)d[propertyKey] == filterValue);

        var json = JsonSerializer.Serialize(message, serializerOptions);
        data.EventBody = new BinaryData(json);

        await sut.ProcessBatchAsync(
            [data],
            partition,
            cancellationToken);

        _ = processor
            .DidNotReceiveWithAnyArgs()
            .ProcessAsync(default, default, default);
    }

    [Theory, AutoNSubstituteData]
    public async Task ProcessBatchAsync_Should_Call_Processor_When_Filter_Does_Match(
        [Frozen] ILogger<TProcessor> logger,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubBatchHandler<TMessage, TProcessor> sut,
        EventData data,
        TMessage message,
        EventProcessorPartition partition,
        string propertyKey,
        string propertyValue,
        string filterValue,
        CancellationToken cancellationToken)
    {
        filters.Clear();
        data.Properties.Add(propertyKey, propertyValue);
        filters.Add(d => (string)d[propertyKey] == propertyValue);

        var json = JsonSerializer.Serialize(message, serializerOptions);
        data.EventBody = new BinaryData(json);

        await sut.ProcessBatchAsync(
             [data],
             partition,
             cancellationToken);

        _ = processor
            .Received(1)
            .ProcessAsync(
                message,
                Arg.Any<EventHubMetadata>(),
                cancellationToken);

        processor
            .ReceivedCallWithArgument<EventHubMetadata>()
            .Should()
            .BeEquivalentTo(
                EventHubMetadata.Create(data, partition.PartitionId));
    }


    [Theory, AutoNSubstituteData]
    public async Task ProcessErrorAsync_Should_Call_Logger(
        [Frozen] ILogger<TProcessor> logger,
        [Frozen, Substitute] TProcessor processor,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubBatchHandler<TMessage, TProcessor> sut,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await sut.ProcessErrorAsync(
            exception,
            cancellationToken);

        logger
            .Received(1)
            .FailedToProcessMessage(
                nameof(TMessage),
                nameof(TProcessor),
                exception);
    }

    [Theory, AutoNSubstituteData]
    public async Task ProcessErrorAsync_Should_Call_Processor(
        [Frozen] ILogger<TProcessorWithErrorHandling> logger,
        [Frozen, Substitute] TProcessorWithErrorHandling processor,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubBatchHandler<TMessage, TProcessorWithErrorHandling> sut,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await sut.ProcessErrorAsync(
            exception,
            cancellationToken);

        _ = processor
            .Received(1)
            .ProcessErrorAsync(
                exception,
                cancellationToken);
    }
}
