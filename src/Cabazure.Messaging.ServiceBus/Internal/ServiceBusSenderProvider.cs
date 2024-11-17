using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public interface IServiceBusSenderProvider
{
    ServiceBusSender GetSender<TMessage>(
        string? connectionName = null);
}

public class ServiceBusSenderProvider(
    IEnumerable<ServiceBusPublisherRegistration> registrations,
    IServiceBusClientProvider clientProvider)
    : IServiceBusSenderProvider
    , IAsyncDisposable
{
    private sealed record SenderKey(string? Connection, string Topic);
    private readonly ConcurrentDictionary<SenderKey, ServiceBusSender> senders = new();
    private readonly Dictionary<Type, ServiceBusPublisherRegistration> publishers
        = registrations.ToDictionary(r => r.Type);

    public ServiceBusSender GetSender<TMessage>(
        string? connectionName = null)
    {
        if (!publishers.TryGetValue(typeof(TMessage), out var publisher))
        {
            throw new ArgumentException(
                $"Type {typeof(TMessage).Name} not configured as a ServiceBus publisher");
        }

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
