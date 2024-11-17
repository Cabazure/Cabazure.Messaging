using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Internal;

public interface IServiceBusClientProvider
{
    ServiceBusClient GetClient(
        string? connectionName = null);
}

public class ServiceBusClientProvider(
    IOptionsMonitor<CabazureServiceBusOptions> monitor)
    : IServiceBusClientProvider
    , IAsyncDisposable
{
    private sealed record ClientKey(string? Connection);
    private readonly ConcurrentDictionary<ClientKey, ServiceBusClient> clients = new();

    public ServiceBusClient GetClient(
        string? connectionName)
        => clients.GetOrAdd(
            new(connectionName),
            CreateClient);

    private ServiceBusClient CreateClient(
        ClientKey key)
        => monitor.Get(key.Connection) switch
        {
            { FullyQualifiedNamespace: { } n, Credential: { } c } => new(n, c),
            { ConnectionString: { } cs } => new(cs),
            _ => throw new ArgumentException(
                $"Missing configuration for Service Bus connection `{key.Connection}`")
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
