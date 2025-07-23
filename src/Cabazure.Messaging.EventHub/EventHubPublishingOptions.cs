namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Represents publishing options specific to Azure Event Hubs, extending the base publishing options with Event Hub-specific settings.
/// </summary>
public class EventHubPublishingOptions : PublishingOptions
{
    public string? PartitionId { get; set; }
}
