namespace Cabazure.Messaging.StorageQueue;

public class StorageQueuePublishingOptions : PublishingOptions
{
    public TimeSpan? VisibilityTimeout { get; set; }
    
    public TimeSpan? TimeToLive { get; set; }
}
