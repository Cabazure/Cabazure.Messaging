namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Represents publishing options specific to Azure Event Hubs, extending the base publishing options with Event Hub-specific settings.
/// </summary>
public class EventHubPublishingOptions : PublishingOptions
{
    /// <summary>
    /// Gets or sets the specific partition identifier to send the message to; if not specified, the Event Hub will automatically assign a partition based on the partition key.
    /// </summary>
    public string? PartitionId { get; set; }
}
