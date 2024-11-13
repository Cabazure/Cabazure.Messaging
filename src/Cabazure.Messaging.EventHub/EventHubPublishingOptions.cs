namespace Cabazure.Messaging.EventHub;

public class EventHubPublishingOptions : PublishingOptions
{
    public string? PartitionId { get; set; }
}
