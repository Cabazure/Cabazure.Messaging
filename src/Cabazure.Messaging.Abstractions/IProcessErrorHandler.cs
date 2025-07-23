namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for handling errors that occur during message processing.
/// </summary>
public interface IProcessErrorHandler
{
    /// <summary>
    /// Handles an error that occurred during message processing asynchronously.
    /// </summary>
    /// <param name="exception">The exception that was thrown during message processing.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous error handling operation.</returns>
    Task ProcessErrorAsync(
        Exception exception,
        CancellationToken cancellationToken);
}
