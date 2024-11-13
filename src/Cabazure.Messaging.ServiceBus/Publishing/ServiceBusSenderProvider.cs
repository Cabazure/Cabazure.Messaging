using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Publishing;

public sealed class ServiceBusSenderProvider(
    IOptionsMonitor<CabazureServiceBusOptions> options,
    IEnumerable<ServcieBusPublisherRegistration> registrations,
    IServiceBusClientProvider clientProvider)
    : IServiceBusSenderProvider
    , IAsyncDisposable
{
    private record SenderKey(string? Connection, string Topic);
    private readonly ConcurrentDictionary<SenderKey, ServiceBusSender> senders = new();
    private readonly Dictionary<Type, ServcieBusPublisherRegistration> publishers
        = registrations.ToDictionary(r => r.Type);

    public ServiceBusSender GetSender<T>(
        string? connectionName = null)
    {
        if (!publishers.TryGetValue(typeof(T), out var publisher))
        {
            throw new ArgumentException(
                $"Type {typeof(T).Name} not configured as a ServiceBus publisher");
        }

        var config = options.Get(connectionName);

        return senders.GetOrAdd(
            new(connectionName, publisher.TopicName),
            key => clientProvider
                .GetClient(key.Connection)
                .CreateSender(key.Topic));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in senders.Values)
        {
            await sender.DisposeAsync();
        }
    }
}