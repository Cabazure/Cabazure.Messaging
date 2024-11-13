using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.Hosting;

namespace Cabazure.Messaging.EventHub.Processing;

public class EventHubProcessorService<TMessage, TProcessor>(
    TProcessor processor,
    EventProcessorClient client,
    JsonSerializerOptions serializerOptions,
    List<Func<IDictionary<string, object>, bool>> filters)
    : IMessageProcessorService<TProcessor>
    , IHostedService
    where TProcessor : class, IMessageProcessor<TMessage>
{
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
        if (!filters.TrueForAll(f => f.Invoke(args.Data.Properties)))
        {
            return;
        }

        var message = args.Data.EventBody
            .ToObjectFromJson<TMessage>(serializerOptions);

        var metadata = new MessageMetadata
        {
            MessageId = args.Data.MessageId,
            CorrelationId = args.Data.CorrelationId,
            ContentType = args.Data.ContentType,
            EnqueuedTime = args.Data.EnqueuedTime,
            PartitionKey = args.Data.PartitionKey,
            Properties = args.Data.Properties,
        };

        await processor.ProcessAsync(
            message!,
            metadata,
            args.CancellationToken);
    }

    private Task OnProcessErrorAsync(ProcessErrorEventArgs args)
    {
        //telemetry.MessageProcessingFailed(args.Exception);

        //if (args.Exception
        //    is UnauthorizedAccessException
        //    or EventHubsException { Reason: EventHubsException.FailureReason.ResourceNotFound })
        //{
        //    IsFaulted = true;
        //    await StopProcessingAsync(CancellationToken.None);
        //}
        return Task.CompletedTask;
    }
}
