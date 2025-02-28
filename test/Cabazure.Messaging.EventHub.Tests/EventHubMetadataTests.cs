using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.Tests;

public class EventHubMetadataTests
{
    [Theory, AutoNSubstituteData]
    public void Create_Should_Map_Properties_Correctly(
        EventData eventData,
        string partitionId)
        => EventHubMetadata
            .Create(eventData, partitionId)
            .Should()
            .BeEquivalentTo(new EventHubMetadata
            {
                ContentType = eventData.ContentType,
                CorrelationId = eventData.CorrelationId,
                EnqueuedTime = eventData.EnqueuedTime,
                MessageId = eventData.MessageId,
                Offset = eventData.Offset,
                PartitionKey = eventData.PartitionKey,
                PartitionId = partitionId,
                Properties = eventData.Properties.ToDictionary(),
                SequenceNumber = eventData.SequenceNumber,
            });
}
