namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Defines a factory for creating Storage Queue publishers for different message types.
/// </summary>
public interface IStorageQueuePublisherFactory
{
    IStorageQueuePublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
