using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub;

public class EventHubMetadata : MessageMetadata
{
    public required string PartitionId { get; init; }

    public required long SequenceNumber { get; init; }

    public required string OffsetString { get; init; }

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
            OffsetString = eventData.OffsetString,
            Properties = new Dictionary<string, object>(
                eventData.Properties),
        };
}
