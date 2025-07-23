using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Represents metadata specific to Azure Event Hub messages, extending the base message metadata with Event Hub-specific properties.
/// </summary>
public class EventHubMetadata : MessageMetadata
{
    /// <summary>
    /// Gets or sets the identifier of the partition where the message was enqueued.
    /// </summary>
    public required string PartitionId { get; init; }

    /// <summary>
    /// Gets or sets the sequence number assigned to the message by the Event Hub.
    /// </summary>
    public long SequenceNumber { get; init; }

    /// <summary>
    /// Gets or sets the offset of the message within the Event Hub partition.
    /// </summary>
    public long Offset { get; init; }

    /// <summary>
    /// Creates an EventHubMetadata instance from Azure Event Hub EventData and partition information.
    /// </summary>
    /// <param name="eventData">The Event Hub event data containing the message metadata.</param>
    /// <param name="partitionId">The identifier of the partition where the message was received.</param>
    /// <returns>A new EventHubMetadata instance populated with data from the EventData.</returns>
    public static EventHubMetadata Create(
        EventData eventData,
        string partitionId)
        => new()
        {
            MessageId = eventData.MessageId,
            ContentType = eventData.ContentType,
            CorrelationId = eventData.CorrelationId,
            EnqueuedTime = eventData.EnqueuedTime,
            PartitionKey = eventData.PartitionKey,
            PartitionId = partitionId,
            SequenceNumber = eventData.SequenceNumber,
            Offset = eventData.Offset,
            Properties = new Dictionary<string, object>(
                eventData.Properties),
        };
}
