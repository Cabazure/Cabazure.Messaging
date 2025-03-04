using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.StorageQueue.Internal;

public class StorageQueueProcessorService<TMessage, TProcessor>(
    TimeProvider timeProvider,
    ILogger<TProcessor> logger,
    TProcessor processor,
    StorageQueueProcessorOptions options,
    JsonSerializerOptions serializerOptions,
    QueueClient queueClient)
    : BackgroundService
    , IMessageProcessorService<TProcessor>
    where TProcessor : IMessageProcessor<TMessage>
{
    public TProcessor Processor { get; } = processor;

    public bool IsRunning { get; private set; }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        IsRunning = true;
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        IsRunning = false;
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.CreateIfNotExists)
        {
            await queueClient.CreateIfNotExistsAsync(
                cancellationToken: stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await queueClient.ReceiveMessagesAsync(stoppingToken);
            foreach (var message in messages.Value)
            {
                try
                {
                    if (message.Body.ToObjectFromJson<TMessage>(serializerOptions) is { } obj)
                    {
                        var metadata = StorageQueueMetadata.Create(message);
                        await Processor.ProcessAsync(obj, metadata, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    await OnProcessErrorAsync(ex, stoppingToken);
                }

                await queueClient.DeleteMessageAsync(
                    message.MessageId,
                    message.PopReceipt,
                    stoppingToken);
            }

            if (messages.Value.Length == 0)
            {
                await timeProvider.Delay(options.PollingInterval, stoppingToken);
            }
        }
    }

    private async Task OnProcessErrorAsync(
        Exception exception,
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
