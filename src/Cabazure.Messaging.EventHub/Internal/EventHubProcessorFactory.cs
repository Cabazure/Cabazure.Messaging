using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubProcessorFactory
{
    EventProcessorClient Create(
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        EventProcessorClientOptions? options = null);
}

public class EventHubProcessorFactory(
    IOptionsMonitor<CabazureEventHubOptions> monitor,
    IBlobStorageClientProvider storageProvider)
    : IEventHubProcessorFactory
{
    public EventProcessorClient Create(
        string? connectionName,
        string eventHubName,
        string consumerGroup,
        EventProcessorClientOptions? options = null)
        => monitor.Get(connectionName) switch
        {
            { FullyQualifiedNamespace: { } ns, Credential: { } cred }
                => new EventProcessorClient(
                    storageProvider.GetClient(connectionName),
                    consumerGroup,
                    ns,
                    eventHubName,
                    cred,
                    options),
            { ConnectionString: { } cs }
                => new EventProcessorClient(
                    storageProvider.GetClient(connectionName),
                    consumerGroup,
                    cs,
                    eventHubName,
                    options),
            _ => throw new ArgumentException(
                $"Missing configuration for Event Hub connection `{connectionName}`"),
        };
}
