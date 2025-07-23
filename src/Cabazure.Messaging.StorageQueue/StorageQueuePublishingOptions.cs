namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Represents publishing options specific to Azure Storage Queue, extending the base publishing options with Storage Queue-specific settings.
/// </summary>
public class StorageQueuePublishingOptions : PublishingOptions
{
    public TimeSpan? VisibilityTimeout { get; set; }
    
    public TimeSpan? TimeToLive { get; set; }
}
