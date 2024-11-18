using System.Text.Json;
using Azure.Messaging.EventHubs.Processor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubProcessorService<TMessage, TProcessor>(
    ILogger<TProcessor> logger,
    TProcessor processor,
    IEventProcessorClient client,
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

        var metadata = EventHubMetadata
            .Create(args.Data);

        await processor.ProcessAsync(
            message!,
            metadata,
            args.CancellationToken);
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
}
