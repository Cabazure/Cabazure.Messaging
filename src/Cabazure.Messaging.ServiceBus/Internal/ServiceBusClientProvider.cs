using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Internal;

public class ServiceBusClientProvider(
    IOptionsMonitor<CabazureServiceBusOptions> options)
    : IServiceBusClientProvider
    , IAsyncDisposable
{
    private sealed record ClientKey(string? Connection);
    private readonly ConcurrentDictionary<ClientKey, ServiceBusClient> clients = new();
    private readonly ConcurrentDictionary<ClientKey, ServiceBusAdministrationClient> adminClients = new();

    public ServiceBusClient GetClient(
        string? connectionName)
        => clients.GetOrAdd(
            new(connectionName),
            CreateClient);

    public ServiceBusAdministrationClient GetAdminClient(
        string? connectionName)
        => adminClients.GetOrAdd(
            new(connectionName),
            CreateAdminClient);

    private ServiceBusClient CreateClient(
        ClientKey key)
        => options.Get(key.Connection) switch
        {
            { FullyQualifiedNamespace: { } n, Credential: { } c } => new(n, c),
            { ConnectionString: { } cs } => new(cs),
            _ => throw new ArgumentException(
                $"Unknown connection name `{key.Connection}`")
        };

    private ServiceBusAdministrationClient CreateAdminClient(
        ClientKey key)
        => options.Get(key.Connection) switch
        {
            { FullyQualifiedNamespace: { } n, Credential: { } c } => new(n, c),
            { ConnectionString: { } cs } => new(cs),
            _ => throw new ArgumentException(
                $"Unknown connection name `{key.Connection}`")
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
