using Azure.Storage.Queues.Models;

namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Represents metadata specific to Azure Storage Queue messages, extending the base message metadata with Storage Queue-specific properties.
/// </summary>
public class StorageQueueMetadata : MessageMetadata
{
    public long DequeueCount { get; set; }

    public DateTimeOffset? InsertedOn { get; set; }

    public DateTimeOffset? ExpiresOn { get; set; }

    public DateTimeOffset? NextVisibleOn { get; set; }

    public required string PopReceipt { get; set; }

    public static StorageQueueMetadata Create(
        QueueMessage message)
        => new()
        {
            MessageId = message.MessageId,
            DequeueCount = message.DequeueCount,
            EnqueuedTime = message.InsertedOn ?? DateTimeOffset.MinValue,
            InsertedOn = message.InsertedOn,
            ExpiresOn = message.ExpiresOn,
            NextVisibleOn = message.NextVisibleOn,
            PopReceipt = message.PopReceipt,
        };
}
