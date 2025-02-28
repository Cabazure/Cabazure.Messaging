namespace Cabazure.Messaging.StorageQueue;

public interface IStorageQueuePublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    Task PublishAsync(
        TMessage message,
        StorageQueuePublishingOptions options,
        CancellationToken cancellationToken);
}
