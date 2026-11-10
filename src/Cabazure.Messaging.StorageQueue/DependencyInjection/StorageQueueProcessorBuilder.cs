using Cabazure.Messaging.StorageQueue.Internal;

namespace Cabazure.Messaging.StorageQueue.DependencyInjection;

public class StorageQueueProcessorBuilder
{
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromDays(7);

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

    public StorageQueueProcessorBuilder WithRetryDelay(
        TimeSpan retryDelay)
    {
        if (retryDelay < TimeSpan.Zero || retryDelay > MaxRetryDelay)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retryDelay),
                retryDelay,
                "Retry delay must be between zero and seven days.");
        }

        Options.RetryDelay = retryDelay;
        return this;
    }
}
