using Azure.Messaging.EventHubs.Primitives;
using Cabazure.Messaging.EventHub.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubBatchProcessorFactory
{
    IEventHubBatchProcessor<TProcessor> Create<TMessage, TProcessor>(
        TProcessor processor,
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        List<Func<IDictionary<string, object>, bool>> filters,
        BlobContainerOptions containerOptions,
        EventProcessorOptions? processorOptions)
        where TProcessor : IMessageProcessor<TMessage>;
}

public class EventHubBatchProcessorFactory(
    ILoggerFactory loggerProvider,
    IBlobStorageClientProvider storageProvider,
    IOptionsMonitor<CabazureEventHubOptions> monitor) : IEventHubBatchProcessorFactory
{
    public IEventHubBatchProcessor<TProcessor> Create<TMessage, TProcessor>(
        TProcessor processor,
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        List<Func<IDictionary<string, object>, bool>> filters,
        BlobContainerOptions containerOptions,
        EventProcessorOptions? processorOptions)
        where TProcessor : IMessageProcessor<TMessage>
        => monitor.Get(connectionName) switch
        {
            { FullyQualifiedNamespace: { } ns, Credential: { } cred, } o
                => new EventHubBatchProcessor<TMessage, TProcessor>(
                    loggerProvider.CreateLogger<TProcessor>(),
                    processor,
                    o.SerializerOptions,
                    filters,
                    GetCheckpointStore(connectionName, containerOptions),
                    ns,
                    cred,
                    eventHubName,
                    consumerGroup,
                    processorOptions),

            { ConnectionString: { } cs } o
                => new EventHubBatchProcessor<TMessage, TProcessor>(
                    loggerProvider.CreateLogger<TProcessor>(),
                    processor,
                    o.SerializerOptions,
                    filters,
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
