namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for publishing messages of a specific type to messaging services.
/// </summary>
/// <typeparam name="TMessage">The type of message to publish.</typeparam>
public interface IMessagePublisher<in TMessage>
{
    /// <summary>
    /// Publishes a message asynchronously with default options.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken);

    /// <summary>
    /// Publishes a message asynchronously with the specified publishing options.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">The options to use when publishing the message, including metadata and routing information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    Task PublishAsync(
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken);
}
