using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.Internal;

public class StorageQueuePublisherFactory(
    IOptionsMonitor<CabazureStorageQueueOptions> monitor,
    IEnumerable<StorageQueuePublisherRegistration> registrations,
    IStorageQueueClientProvider clientProvider)
    : IStorageQueuePublisherFactory
{
    private readonly Dictionary<Type, StorageQueuePublisherRegistration[]> registrations
        = registrations.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.ToArray());

    public IStorageQueuePublisher<TMessage> Create<TMessage>(
        string? connectionName = null)
    {
        var registration = FindRegistration<TMessage>(connectionName);

        var options = monitor.Get(connectionName);
        var queue = clientProvider
            .GetClient(connectionName)
            .GetQueueClient(registration.QueueName);

        return new StorageQueuePublisher<TMessage>(
            options.SerializerOptions,
            queue);
    }

    private StorageQueuePublisherRegistration FindRegistration<TMessage>(
        string? connectionName)
    {
        if (!registrations.TryGetValue(typeof(TMessage), out var matches))
        {
            throw new ArgumentException(
                $"Type {typeof(TMessage).Name} not configured as a StorageQueue publisher");
        }

        if (matches.Length == 1)
        {
            return matches[0];
        }

        if (matches.FirstOrDefault(m => m.ConnectionName == connectionName) is { } match)
        {
            return match;
        }

        throw new ArgumentException(
            connectionName is { Length: > 0 } c
                ? $"Type {typeof(TMessage).Name} not configured as a StorageQueue publisher for connection '{c}'"
                : $"Type {typeof(TMessage).Name} not configured as a StorageQueue publisher for default connection");
    }
}
