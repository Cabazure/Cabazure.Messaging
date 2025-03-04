namespace Cabazure.Messaging.StorageQueue.Internal;

public class StorageQueueProcessorOptions
{
    public TimeSpan PollingInterval { get; set; }
        = TimeSpan.FromSeconds(5);

    public bool CreateIfNotExists { get; set; }
}
