namespace Cabazure.Messaging.StorageQueue.DependencyInjection;

public class StorageQueueProcessorBuilder
{
    public TimeSpan PollingInterval { get; private set; } = TimeSpan.FromSeconds(5);
    public bool CreateIfNotExists { get; set; } = true;

    public StorageQueueProcessorBuilder WithPollingInterval(
        TimeSpan pollingInterval)
    {
        PollingInterval = pollingInterval;
        return this;
    }

    public StorageQueueProcessorBuilder WithInitialization(
        bool createIfNotExists)
    {
        CreateIfNotExists = createIfNotExists;
        return this;
    }
}
