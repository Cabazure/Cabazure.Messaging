using System.Text.Json;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubBatchProcessor<TMessage, TProcessor>
    : PluggableCheckpointStoreEventProcessor<EventProcessorPartition>
    , IEventHubBatchProcessor<TProcessor>
    where TProcessor : IMessageProcessor<TMessage>
{
    private const int MaxBatchCount = 25;

    private readonly ILogger<TProcessor> logger;
    private readonly JsonSerializerOptions serializerOptions;
    private readonly List<Func<IDictionary<string, object>, bool>> filters;

    public EventHubBatchProcessor(
        ILogger<TProcessor> logger,
        TProcessor processor,
        JsonSerializerOptions serializerOptions,
        List<Func<IDictionary<string, object>, bool>> filters,
        CheckpointStore checkpointStore,
        string fullyQualifiedNamespace,
        TokenCredential credential,
        string eventhubName,
        string consumerGroup,
        EventProcessorOptions? processorOptions)
        : base(
            checkpointStore,
            MaxBatchCount,
            consumerGroup,
            fullyQualifiedNamespace,
            eventhubName,
            credential,
            processorOptions)
    {
        this.logger = logger;
        Processor = processor;
        this.serializerOptions = serializerOptions;
        this.filters = filters;
    }

    public EventHubBatchProcessor(
        ILogger<TProcessor> logger,
        TProcessor processor,
        JsonSerializerOptions serializerOptions,
        List<Func<IDictionary<string, object>, bool>> filters,
        CheckpointStore checkpointStore,
        string connectionString,
        string eventhubName,
        string consumerGroup,
        EventProcessorOptions? processorOptions)
        : base(
            checkpointStore,
            MaxBatchCount,
            consumerGroup,
            connectionString,
            eventhubName,
            processorOptions)
    {
        this.logger = logger;
        Processor = processor;
        this.serializerOptions = serializerOptions;
        this.filters = filters;
    }

    public TProcessor Processor { get; }

    protected override async Task OnProcessingEventBatchAsync(
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

            await Processor.ProcessAsync(
                message!,
                metadata,
                cancellationToken);
        }
    }

    protected override async Task OnProcessingErrorAsync(
        Exception exception,
        EventProcessorPartition partition,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        if (Processor is IProcessErrorHandler handler)
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
