using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace Cabazure.Messaging.ServiceBus.Internal;

public class ServiceBusProcessorService<TMessage, TProcessor>(
    TProcessor processor,
    ServiceBusProcessor client,
    JsonSerializerOptions serializerOptions,
    List<Func<IReadOnlyDictionary<string, object>, bool>> filters)
    : IMessageProcessorService<TProcessor>
    , IHostedService
    where TProcessor : class, IMessageProcessor<TMessage>
{
    private bool isStarted;

    public bool IsRunning => isStarted && client.IsProcessing;

    public TProcessor Processor => processor;

    public async Task StartAsync(
           CancellationToken cancellationToken)
    {
        isStarted = true;
        client.ProcessMessageAsync += OnProcessMessageAsync;
        client.ProcessErrorAsync += OnProcessErrorAsync;

        await client.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        isStarted = false;
        await client.StopProcessingAsync(cancellationToken);

        client.ProcessMessageAsync -= OnProcessMessageAsync;
        client.ProcessErrorAsync -= OnProcessErrorAsync;
    }

    private async Task OnProcessMessageAsync(ProcessMessageEventArgs args)
    {
        if (!filters.TrueForAll(f => f.Invoke(args.Message.ApplicationProperties)))
        {
            return;
        }

        var metadata = ServiceBusMetadata
            .Create(args.Message);

        var message = args.Message.Body
            .ToObjectFromJson<TMessage>(serializerOptions);

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
