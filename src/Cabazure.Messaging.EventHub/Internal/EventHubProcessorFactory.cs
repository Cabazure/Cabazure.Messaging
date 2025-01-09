using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Cabazure.Messaging.EventHub.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubProcessorFactory
{
    IEventHubProcessor Create(
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        EventProcessorClientOptions? processorOptions = null);
}

public class EventHubProcessorFactory(
    IOptionsMonitor<CabazureEventHubOptions> monitor,
    IBlobStorageClientProvider storageProvider)
    : IEventHubProcessorFactory
{
    public IEventHubProcessor Create(
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        EventProcessorClientOptions? processorOptions = null)
        => monitor.Get(connectionName) switch
        {
            { FullyQualifiedNamespace: { } ns, Credential: { } cred }
                => new EventHubProcessorWrapper(
                    new EventProcessorClient(
                        GetContainerClient(connectionName, containerOptions),
                        consumerGroup,
                        ns,
                        eventHubName,
                        cred,
                        processorOptions)),
            {
                ConnectionString: { } cs
            }
                => new EventHubProcessorWrapper(
                    new EventProcessorClient(
                        GetContainerClient(connectionName, containerOptions),
                        consumerGroup,
                        cs,
                        eventHubName,
                        processorOptions)),
            _ => throw new ArgumentException(
                $"Missing configuration for Event Hub connection `{connectionName}`"),
        };

    private BlobContainerClient GetContainerClient(
        string? connectionName,
        BlobContainerOptions options)
    {
        var client = storageProvider.GetClient(connectionName);
        var container = client.GetBlobContainerClient(options.ContainerName);

        if (options.CreateIfNotExist)
        {
            container.CreateIfNotExists();
        }

        return container;
    }
}
