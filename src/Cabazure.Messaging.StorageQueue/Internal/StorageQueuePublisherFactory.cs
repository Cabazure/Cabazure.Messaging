using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.Internal;

public class StorageQueuePublisherFactory(
    IOptionsMonitor<CabazureStorageQueueOptions> monitor,
    IEnumerable<StorageQueuePublisherRegistration> registrations,
    IStorageQueueClientProvider clientProvider)
    : IStorageQueuePublisherFactory
{
    private readonly Dictionary<Type, StorageQueuePublisherRegistration> publishers
        = registrations.ToDictionary(r => r.Type);

    public IStorageQueuePublisher<TMessage> Create<TMessage>(
        string? connectionName = null)
    {
        if (!publishers.TryGetValue(typeof(TMessage), out var publisher))
        {
            throw new ArgumentException(
                $"Type {typeof(TMessage).Name} not configured as a StorageQueue publisher");
        }

        var options = monitor.Get(connectionName);
        var queue = clientProvider
            .GetClient(connectionName)
            .GetQueueClient(publisher.QueueName);

        return new StorageQueuePublisher<TMessage>(
            options.SerializerOptions,
            queue);
    }
}
