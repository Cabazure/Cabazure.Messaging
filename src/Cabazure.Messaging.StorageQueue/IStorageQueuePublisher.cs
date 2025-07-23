namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Defines a contract for publishing messages to Azure Storage Queue with Storage Queue-specific options.
/// </summary>
/// <typeparam name="TMessage">The type of message to publish.</typeparam>
public interface IStorageQueuePublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    /// <summary>
    /// Publishes a message asynchronously to Azure Storage Queue with Storage Queue-specific publishing options.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">The Storage Queue-specific options to use when publishing the message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    Task PublishAsync(
        TMessage message,
        StorageQueuePublishingOptions options,
        CancellationToken cancellationToken);
}
