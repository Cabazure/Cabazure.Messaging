namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Represents publishing options specific to Azure Service Bus, extending the base publishing options with Service Bus-specific settings.
/// </summary>
public class ServiceBusPublishingOptions : PublishingOptions
{
    public TimeSpan? TimeToLive { get; set; }

    public string? SessionId { get; set; }

    public DateTimeOffset? ScheduledEnqueueTime { get; set; }
}
