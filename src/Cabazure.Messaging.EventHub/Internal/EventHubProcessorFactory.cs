using Azure.Messaging.EventHubs.Primitives;
using Cabazure.Messaging.EventHub.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubProcessorFactory
{
    IEventHubProcessor<TProcessor> Create<TMessage, TProcessor>(
        IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        EventProcessorOptions? processorOptions)
        where TProcessor : IMessageProcessor<TMessage>;
}

public class EventHubProcessorFactory(
    IBlobStorageClientProvider storageProvider,
    IOptionsMonitor<CabazureEventHubOptions> monitor)
    : IEventHubProcessorFactory
{
    public IEventHubProcessor<TProcessor> Create<TMessage, TProcessor>(
        IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        EventProcessorOptions? processorOptions)
        where TProcessor : IMessageProcessor<TMessage>
        => monitor.Get(connectionName) switch
        {
            { FullyQualifiedNamespace: { } ns, Credential: { } cred, }
                => new EventHubProcessor<TMessage, TProcessor>(
                    batchHandler,
                    GetCheckpointStore(connectionName, containerOptions),
                    ns,
                    cred,
                    eventHubName,
                    consumerGroup,
                    processorOptions),

            { ConnectionString: { } cs }
                => new EventHubProcessor<TMessage, TProcessor>(
                    batchHandler,
                    GetCheckpointStore(connectionName, containerOptions),
                    cs,
                    eventHubName,
                    consumerGroup,
                    processorOptions),

            _ => throw new ArgumentException(
                $"Missing configuration for Event Hub connection `{connectionName}`"),
        };


    private BlobCheckpointStore GetCheckpointStore(
        string? connectionName,
        BlobContainerOptions options)
    {
        var client = storageProvider.GetClient(connectionName);
        var container = client.GetBlobContainerClient(options.ContainerName);

        if (options.CreateIfNotExist)
        {
            container.CreateIfNotExists();
        }

        return new BlobCheckpointStore(container);
    }
}
