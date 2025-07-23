namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Defines a contract for publishing messages to Azure Storage Queue with Storage Queue-specific options.
/// </summary>
/// <typeparam name="TMessage">The type of message to publish.</typeparam>
public interface IStorageQueuePublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    Task PublishAsync(
        TMessage message,
        StorageQueuePublishingOptions options,
        CancellationToken cancellationToken);
}
