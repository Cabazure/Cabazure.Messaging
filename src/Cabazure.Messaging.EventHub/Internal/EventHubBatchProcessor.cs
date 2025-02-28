using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubBatchProcessor<TProcessor>
{
    string FullyQualifiedNamespace { get; }

    string EventHubName { get; }

    string ConsumerGroup { get; }

    TProcessor Processor { get; }

    bool IsRunning { get; }

    Task StartProcessingAsync(
        CancellationToken cancellationToken);

    Task StopProcessingAsync(
        CancellationToken cancellationToken);
}

public class EventHubBatchProcessor<TMessage, TProcessor>
    : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>
    , IEventHubBatchProcessor<TProcessor>
    where TProcessor : IMessageProcessor<TMessage>
{
    private const int MaxBatchCount = 25;

    private readonly IEventHubBatchHandler<TMessage, TProcessor> batchHandler;

    public EventHubBatchProcessor(
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

    public EventHubBatchProcessor(
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
        => await batchHandler
            .ProcessBatchAsync(
                events,
                partition,
                cancellationToken)
            .ConfigureAwait(false);

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
