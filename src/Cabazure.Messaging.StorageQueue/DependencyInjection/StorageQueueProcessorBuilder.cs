using Cabazure.Messaging.StorageQueue.Internal;

namespace Cabazure.Messaging.StorageQueue.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Storage Queue processors with options such as polling intervals and visibility timeouts.
/// </summary>
public class StorageQueueProcessorBuilder
{
    /// <summary>
    /// Gets or sets the processor options that control the behavior of the Storage Queue processor.
    /// </summary>
    public StorageQueueProcessorOptions Options { get; set; } = new();

    /// <summary>
    /// Configures the polling interval for checking the queue for new messages.
    /// </summary>
    /// <param name="pollingInterval">The time interval between polling operations.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StorageQueueProcessorBuilder WithPollingInterval(
        TimeSpan pollingInterval)
    {
        Options.PollingInterval = pollingInterval;
        return this;
    }

    /// <summary>
    /// Configures whether the queue should be created if it does not exist.
    /// </summary>
    /// <param name="createIfNotExists">True to create the queue if it doesn't exist; otherwise, false.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StorageQueueProcessorBuilder WithInitialization(
        bool createIfNotExists)
    {
        Options.CreateIfNotExists = createIfNotExists;
        return this;
    }
}
