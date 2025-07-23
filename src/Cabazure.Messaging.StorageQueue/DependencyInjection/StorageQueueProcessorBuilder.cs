using Cabazure.Messaging.StorageQueue.Internal;

namespace Cabazure.Messaging.StorageQueue.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Storage Queue processors with options such as polling intervals and visibility timeouts.
/// </summary>
public class StorageQueueProcessorBuilder
{
    public StorageQueueProcessorOptions Options { get; set; } = new();

    public StorageQueueProcessorBuilder WithPollingInterval(
        TimeSpan pollingInterval)
    {
        Options.PollingInterval = pollingInterval;
        return this;
    }

    public StorageQueueProcessorBuilder WithInitialization(
        bool createIfNotExists)
    {
        Options.CreateIfNotExists = createIfNotExists;
        return this;
    }
}
