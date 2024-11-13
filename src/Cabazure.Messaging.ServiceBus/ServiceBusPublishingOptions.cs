namespace Cabazure.Messaging.ServiceBus;

public class ServiceBusPublishingOptions : PublishingOptions
{
    public TimeSpan? TimeToLive { get; set; }

    public string? SessionId { get; set; }

    public DateTimeOffset? ScheduledEnqueueTime { get; set; }
}
