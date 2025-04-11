using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Messaging.EventHubs.Processor;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubProcessor<TMessage, TProcessor>
    : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>
    , IEventHubProcessor<TProcessor>
    where TProcessor : IMessageProcessor<TMessage>
{
    private const int MaxBatchCount = 25;

    private readonly IEventHubBatchHandler<TMessage, TProcessor> batchHandler;

    public EventHubProcessor(
        IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        CheckpointStore checkpointStore,
        string fullyQualifiedNamespace,
        TokenCredential credential,
        string eventhubName,
        string consumerGroup,
        EventProcessorOptions? processorOptions)
        : base(
            checkpointStore: checkpointStore,
            eventBatchMaximumCount: MaxBatchCount,
            consumerGroup: consumerGroup,
            fullyQualifiedNamespace: fullyQualifiedNamespace,
            eventHubName: eventhubName,
            credential: credential,
            options: processorOptions)
        => this.batchHandler = batchHandler;

    public EventHubProcessor(
        IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        CheckpointStore checkpointStore,
        string connectionString,
        string eventhubName,
        string consumerGroup,
        EventProcessorOptions? processorOptions)
        : base(
            checkpointStore: checkpointStore,
            eventBatchMaximumCount: MaxBatchCount,
            consumerGroup: consumerGroup,
            connectionString: connectionString,
            eventHubName: eventhubName,
            options: processorOptions)
        => this.batchHandler = batchHandler;

    public TProcessor Processor => batchHandler.Processor;

    protected override async Task OnProcessingEventBatchAsync(
        IEnumerable<EventData> events,
        EventProcessorPartition partition,
        CancellationToken cancellationToken)
    {
        var lastEvent = await batchHandler
            .ProcessBatchAsync(
                events,
                partition,
                cancellationToken)
            .ConfigureAwait(false);

        if (lastEvent != null)
        {
            await UpdateCheckpointAsync(
                    partition.PartitionId,
                    CheckpointPosition.FromEvent(lastEvent),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    protected override async Task OnProcessingErrorAsync(
        Exception exception,
        EventProcessorPartition partition,
        string operationDescription,
        CancellationToken cancellationToken)
        => await batchHandler
            .ProcessErrorAsync(
                exception,
                cancellationToken)
            .ConfigureAwait(false);
}
