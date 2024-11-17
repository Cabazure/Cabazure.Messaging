using System.Collections.Concurrent;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubProducerClientProvider
{
    EventHubProducerClient GetClient(
        string? connectionName,
        string eventHubName);
}

public class EventHubProducerClientProvider(
    IOptionsMonitor<CabazureEventHubOptions> options)
    : IEventHubProducerClientProvider
    , IAsyncDisposable
{
    private sealed record ClientKey(string? Connection, string EventHub);
    private readonly ConcurrentDictionary<ClientKey, EventHubProducerClient> clients = new();

    public EventHubProducerClient GetClient(
        string? connectionName,
        string eventHubName)
        => clients.GetOrAdd(
            new(connectionName, eventHubName),
            CreateClient);

    private EventHubProducerClient CreateClient(
        ClientKey clientKey)
        => options.Get(clientKey.Connection) switch
        {
            { FullyQualifiedNamespace: { } n, Credential: { } c } => new(n, clientKey.EventHub, c),
            { ConnectionString: { } cs } => new(cs, clientKey.EventHub),
            _ => throw new ArgumentException(
                $"Missing configuration for Event Hub connection `{clientKey.Connection}`"),
        };

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        foreach (var client in clients.Values)
        {
            await client.DisposeAsync();
        }
    }
}
