namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for processing messages of a specific type.
/// </summary>
/// <typeparam name="TMessage">The type of message to process.</typeparam>
public interface IMessageProcessor<in TMessage>
{
    /// <summary>
    /// Processes a message asynchronously with its associated metadata.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="metadata">The metadata associated with the message, including identifiers and timing information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous processing operation.</returns>
    Task ProcessAsync(
        TMessage message,
        MessageMetadata metadata,
        CancellationToken cancellationToken);
}
