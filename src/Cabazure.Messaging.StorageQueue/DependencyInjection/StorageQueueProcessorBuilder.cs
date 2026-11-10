using Cabazure.Messaging.StorageQueue.Internal;

namespace Cabazure.Messaging.StorageQueue.DependencyInjection;

public class StorageQueueProcessorBuilder
{
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromDays(7);
    private static readonly TimeSpan MinMessageClaimDuration = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxMessageClaimDuration = TimeSpan.FromDays(7);

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

    public StorageQueueProcessorBuilder WithMessageClaimDuration(
        TimeSpan messageClaimDuration)
    {
        if (messageClaimDuration < MinMessageClaimDuration
            || messageClaimDuration > MaxMessageClaimDuration)
        {
            throw new ArgumentOutOfRangeException(
                nameof(messageClaimDuration),
                messageClaimDuration,
                "Message claim duration must be between one second and seven days.");
        }

        Options.MessageClaimDuration = messageClaimDuration;
        return this;
    }
}
