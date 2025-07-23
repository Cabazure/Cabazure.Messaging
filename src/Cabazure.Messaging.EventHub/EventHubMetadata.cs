using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Represents metadata specific to Azure Event Hub messages, extending the base message metadata with Event Hub-specific properties.
/// </summary>
public class EventHubMetadata : MessageMetadata
{
    public required string PartitionId { get; init; }

    public long SequenceNumber { get; init; }

    public long Offset { get; init; }

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
