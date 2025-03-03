namespace Cabazure.Messaging.StorageQueue;

public interface IStorageQueuePublisherFactory
{
    IStorageQueuePublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
