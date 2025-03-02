using Azure.Storage.Queues.Models;

namespace Cabazure.Messaging.StorageQueue.Tests;

public class StorageQueueMetadataTests
{
    [Theory, AutoNSubstituteData]
    public void Create_Should_Map_Properties_Correctly(
        QueueMessage message)
        => StorageQueueMetadata
            .Create(message)
            .Should()
            .BeEquivalentTo(new StorageQueueMetadata
            {
                EnqueuedTime = message.InsertedOn!.Value,
                MessageId = message.MessageId,
                DequeueCount = message.DequeueCount,
                PopReceipt = message.PopReceipt,
                ExpiresOn = message.ExpiresOn,
                InsertedOn = message.InsertedOn,
                NextVisibleOn = message.NextVisibleOn,
            });
}
