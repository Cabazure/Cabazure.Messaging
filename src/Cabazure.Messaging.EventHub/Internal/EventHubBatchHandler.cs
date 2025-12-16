using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubBatchHandler<TMessage, TProcessor>
{
    public TProcessor Processor { get; }

    Task<EventData?> ProcessBatchAsync(
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

    public async Task<EventData?> ProcessBatchAsync(
       IEnumerable<EventData> events,
       EventProcessorPartition partition,
       CancellationToken cancellationToken)
    {
        EventData? lastEvent = null;
        foreach (var evt in events.OrderBy(e => e.SequenceNumber))
        {
            lastEvent = evt;

            try
            {
                if (!filters.TrueForAll(f => f.Invoke(evt.Properties)))
                {
                    continue;
                }

                var message = evt.EventBody
                    .ToObjectFromJson<TMessage>(serializerOptions);

                if (message != null)
                {
                    var metadata = EventHubMetadata
                        .Create(evt, partition.PartitionId);

                    await processor
                        .ProcessAsync(
                            message!,
                            metadata,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await ProcessErrorAsync(ex, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return lastEvent;
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
