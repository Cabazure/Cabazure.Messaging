using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubConsumerClientFactory
{
    IEventHubConsumerClient Create(
        string? connectionName,
        string eventHubName,
        string consumerGroupName);
}

public class EventHubConsumerClientFactory(
    IOptionsMonitor<CabazureEventHubOptions> monitor)
    : IEventHubConsumerClientFactory
{
    public IEventHubConsumerClient Create(
        string? connectionName,
        string eventHubName,
        string consumerGroupName)
        => monitor.Get(connectionName) switch
        {
            { FullyQualifiedNamespace: { } ns, Credential: { } cred, }
                => new EventHubConsumerClientWrapper(
                    new EventHubConsumerClient(
                        consumerGroup: consumerGroupName,
                        fullyQualifiedNamespace: ns,
                        eventHubName: eventHubName,
                        credential: cred)),

            { ConnectionString: { } cs }
                => new EventHubConsumerClientWrapper(
                    new EventHubConsumerClient(
                        consumerGroup: consumerGroupName,
                        connectionString: cs,
                        eventHubName: eventHubName)),

            _ => throw new ArgumentException(
                $"Missing configuration for Event Hub connection `{connectionName}`"),
        };
}
