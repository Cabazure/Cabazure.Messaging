using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubProducerClientFactory
{
    EventHubProducerClient Create(
        string? connectionName,
        string eventHubName);
}

public class EventHubProducerClientFactory(
    IOptionsMonitor<CabazureEventHubOptions> options)
    : IEventHubProducerClientFactory
{
    public EventHubProducerClient Create(
        string? connectionName,
        string eventHubName)
        => options.Get(connectionName) switch
        {
            { FullyQualifiedNamespace: { } n, Credential: { } c } => new(n, eventHubName, c),
            { ConnectionString: { } cs } => new(cs, eventHubName),
            _ => throw new ArgumentException(
                $"Missing configuration for Event Hub connection `{connectionName}`"),
        };
}
