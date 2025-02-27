using System.Collections.Concurrent;
using System.Text.Json;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubProcessorService<TMessage, TProcessor>(
    ILogger<TProcessor> logger,
    TProcessor processor,
    IEventHubProcessor client,
    JsonSerializerOptions serializerOptions,
    List<Func<IDictionary<string, object>, bool>> filters,
    TimeSpan checkpointMaxAge,
    int checkpointMaxCount)
    : IMessageProcessorService<TProcessor>
    , IHostedService
    where TProcessor : class, IMessageProcessor<TMessage>
{
    private readonly ConcurrentDictionary<string, int> partitionEventCount = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> partitionCheckpoints = new();

    public bool IsRunning => client.IsRunning;

    public TProcessor Processor => processor;

    public async Task StartAsync(
           CancellationToken cancellationToken)
    {
        client.ProcessEventAsync += OnProcessMessageAsync;
        client.ProcessErrorAsync += OnProcessErrorAsync;

        await client.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopProcessingAsync(cancellationToken);

        client.ProcessEventAsync -= OnProcessMessageAsync;
        client.ProcessErrorAsync -= OnProcessErrorAsync;
    }

    private async Task OnProcessMessageAsync(ProcessEventArgs args)
    {
        try
        {
            if (!filters.TrueForAll(f => f.Invoke(args.Data.Properties)))
            {
                return;
            }

            var message = args.Data.EventBody
                .ToObjectFromJson<TMessage>(serializerOptions);

            var metadata = EventHubMetadata
                .Create(args.Data);

            await processor.ProcessAsync(
                message!,
                metadata,
                args.CancellationToken);

            await UpdateCheckpointAsync(args);
        }
        catch (Exception ex)
        {
            await OnProcessErrorAsync(
                new ProcessErrorEventArgs(
                    args.Partition.PartitionId,
                    nameof(OnProcessMessageAsync),
                    ex,
                    args.CancellationToken));
        }
    }

    private async Task OnProcessErrorAsync(ProcessErrorEventArgs args)
    {
        if (processor is IProcessErrorHandler handler)
        {
            await handler.ProcessErrorAsync(
                args.Exception,
                args.CancellationToken);
        }
        else
        {
            logger.FailedToProcessMessage(
                typeof(TMessage).Name,
                typeof(TProcessor).Name,
                args.Exception);
        }
    }

    private async Task UpdateCheckpointAsync(ProcessEventArgs args)
    {
        string partition = args.Partition.PartitionId;

        var eventsSinceLastCheckpoint = partitionEventCount.AddOrUpdate(
            key: partition,
            addValue: 1,
            updateValueFactory: (_, currentValue) => currentValue + 1);
        var lastCheckpoint = partitionCheckpoints.GetOrAdd(
            key: partition,
            DateTimeOffset.UtcNow);

        if (eventsSinceLastCheckpoint >= checkpointMaxCount || lastCheckpoint.Add(checkpointMaxAge) < DateTimeOffset.UtcNow)
        {
            await args.UpdateCheckpointAsync();

            partitionEventCount[partition] = 0;
            partitionCheckpoints[partition] = DateTimeOffset.UtcNow;
        }
    }
}
