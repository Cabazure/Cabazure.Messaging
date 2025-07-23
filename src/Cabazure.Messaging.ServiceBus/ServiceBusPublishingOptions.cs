namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Represents publishing options specific to Azure Service Bus, extending the base publishing options with Service Bus-specific settings.
/// </summary>
public class ServiceBusPublishingOptions : PublishingOptions
{
    /// <summary>
    /// Gets or sets the time-to-live duration for the message. If not specified, the default TTL configured on the queue or topic will be used.
    /// </summary>
    public TimeSpan? TimeToLive { get; set; }

    /// <summary>
    /// Gets or sets the session identifier for the message when using session-aware queues or topics.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled time when the message should be made available for processing. If not specified, the message will be available immediately.
    /// </summary>
    public DateTimeOffset? ScheduledEnqueueTime { get; set; }
}
