using Microsoft.Extensions.Hosting;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubProcessorService<TMessage, TProcessor>(
    IEventHubBatchProcessor<TProcessor> batchProcessor)
    : IMessageProcessorService<TProcessor>
    , IHostedService
    where TProcessor : class, IMessageProcessor<TMessage>
{
    public bool IsRunning => batchProcessor.IsRunning;

    public TProcessor Processor => batchProcessor.Processor;

    public async Task StartAsync(
        CancellationToken cancellationToken)
        => await batchProcessor
            .StartProcessingAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task StopAsync(CancellationToken cancellationToken)
        => await batchProcessor
            .StopProcessingAsync(cancellationToken)
            .ConfigureAwait(false);
}
