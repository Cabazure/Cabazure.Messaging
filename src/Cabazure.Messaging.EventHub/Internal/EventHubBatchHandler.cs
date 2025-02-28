using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubBatchHandler<TMessage, TProcessor>
{
    public TProcessor Processor { get; }

    Task ProcessBatchAsync(
        IEnumerable<EventData> events,
        EventProcessorPartition partition,
        CancellationToken cancellationToken);

    Task ProcessErrorAsync(
        Exception exception,
        CancellationToken cancellationToken);
}

public class EventHubBatchHandler<TMessage, TProcessor>(
    ILogger<TProcessor> logger,
    TProcessor processor,
    JsonSerializerOptions serializerOptions,
    List<Func<IDictionary<string, object>, bool>> filters)
    : IEventHubBatchHandler<TMessage, TProcessor>
    where TProcessor : IMessageProcessor<TMessage>
{
    public TProcessor Processor => processor;

    public async Task ProcessBatchAsync(
       IEnumerable<EventData> events,
       EventProcessorPartition partition,
       CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            if (!filters.TrueForAll(f => f.Invoke(evt.Properties)))
            {
                continue;
            }

            var message = evt.EventBody
                .ToObjectFromJson<TMessage>(serializerOptions);

            var metadata = EventHubMetadata
                .Create(evt, partition.PartitionId);

            await processor.ProcessAsync(
                message!,
                metadata,
                cancellationToken);
        }
    }

    public async Task ProcessErrorAsync(
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (processor is IProcessErrorHandler handler)
        {
            await handler.ProcessErrorAsync(
                exception,
                cancellationToken);
        }
        else
        {
            logger.FailedToProcessMessage(
                typeof(TMessage).Name,
                typeof(TProcessor).Name,
                exception);
        }
    }
}
