using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Internal;

public class ServiceBusSenderProvider(
    IOptionsMonitor<CabazureServiceBusOptions> options,
    IEnumerable<ServiceBusPublisherRegistration> registrations,
    IServiceBusClientProvider clientProvider)
    : IServiceBusSenderProvider
    , IAsyncDisposable
{
    private sealed record SenderKey(string? Connection, string Topic);
    private readonly ConcurrentDictionary<SenderKey, ServiceBusSender> senders = new();
    private readonly Dictionary<Type, ServiceBusPublisherRegistration> publishers
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
            new(connectionName, publisher.TopicOrQueueName),
            key => clientProvider
                .GetClient(key.Connection)
                .CreateSender(key.Topic));
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        foreach (var sender in senders.Values)
        {
            await sender.DisposeAsync();
        }
    }
}
