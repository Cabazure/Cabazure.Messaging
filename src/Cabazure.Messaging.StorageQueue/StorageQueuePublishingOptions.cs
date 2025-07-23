namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Represents publishing options specific to Azure Storage Queue, extending the base publishing options with Storage Queue-specific settings.
/// </summary>
public class StorageQueuePublishingOptions : PublishingOptions
{
    /// <summary>
    /// Gets or sets the visibility timeout for the message, specifying how long 
    /// the message will be invisible after being dequeued. If not specified, the 
    /// default visibility timeout configured on the queue will be used.
    /// </summary>
    public TimeSpan? VisibilityTimeout { get; set; }
    
    /// <summary>
    /// Gets or sets the time-to-live duration for the message. If not specified, the message will not expire.
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }
}
