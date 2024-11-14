using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub;

public class EventHubMetadata : MessageMetadata
{
    public long SequenceNumber { get; init; }

    public long Offset { get; init; }

    public static EventHubMetadata Create(
        EventData eventData)
        => new()
        {
            MessageId = eventData.MessageId,
            ContentType = eventData.ContentType,
            CorrelationId = eventData.CorrelationId,
            EnqueuedTime = eventData.EnqueuedTime,
            PartitionKey = eventData.PartitionKey,
            SequenceNumber = eventData.SequenceNumber,
            Offset = eventData.Offset,
            Properties = new Dictionary<string, object>(
                eventData.Properties),
        };
}
