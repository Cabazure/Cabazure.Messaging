namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for a service that hosts and manages a message processor.
/// </summary>
/// <typeparam name="TProcessor">The type of message processor.</typeparam>
public interface IMessageProcessorService<out TProcessor>
{
    /// <summary>
    /// Gets the message processor instance managed by this service.
    /// </summary>
    TProcessor Processor { get; }

    /// <summary>
    /// Gets a value indicating whether the processor service is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the message processor service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    Task StartAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Stops the message processor service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    Task StopAsync(
        CancellationToken cancellationToken);
}
