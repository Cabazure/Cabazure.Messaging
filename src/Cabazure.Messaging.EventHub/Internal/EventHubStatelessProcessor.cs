﻿using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubStatelessProcessor<TMessage, TProcessor>(
    IEventHubConsumerClient client,
    ILogger<TProcessor> logger,
    TProcessor processor,
    JsonSerializerOptions serializerOptions,
    ReadEventOptions? readOptions,
    List<Func<IDictionary<string, object>, bool>> filters)
    : BackgroundService
    , IEventHubProcessor<TProcessor>
    , IAsyncDisposable
    where TProcessor : IMessageProcessor<TMessage>
{
    public string FullyQualifiedNamespace => client.FullyQualifiedNamespace;

    public string EventHubName => client.EventHubName;

    public string ConsumerGroup => client.ConsumerGroup;

    public TProcessor Processor => processor;

    public bool IsRunning { get; private set; }

    public Task StartProcessingAsync(CancellationToken cancellationToken)
        => StartAsync(cancellationToken);

    public Task StopProcessingAsync(CancellationToken cancellationToken)
        => StopAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            IsRunning = true;

            // Ensure background task is running asynchronously
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                var events = client.ReadEventsAsync(
                    startReadingAtEarliestEvent: false,
                    readOptions: readOptions,
                    cancellationToken: stoppingToken);

                await foreach (var evt in events.WithCancellation(stoppingToken))
                {
                    await ProcessMessageAsync(evt.Data, evt.Partition, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            await ProcessErrorAsync(ex, stoppingToken);
        }
        finally
        {
            IsRunning = false;
        }
    }

    private async Task ProcessMessageAsync(
        EventData data,
        PartitionContext partition,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!filters.TrueForAll(f => f.Invoke(data.Properties)))
            {
                return;
            }

            var message = data.EventBody
                .ToObjectFromJson<TMessage>(serializerOptions);

            var metadata = EventHubMetadata
                .Create(data, partition.PartitionId);

            await processor.ProcessAsync(
                message!,
                metadata,
                cancellationToken);
        }
        catch (Exception exception)
        {
            await ProcessErrorAsync(
                exception,
                cancellationToken);
        }
    }

    private async Task ProcessErrorAsync(
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

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await client.DisposeAsync();
    }
}
