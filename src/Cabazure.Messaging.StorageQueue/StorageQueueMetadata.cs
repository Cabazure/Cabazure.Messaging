using Azure.Storage.Queues.Models;

namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Represents metadata specific to Azure Storage Queue messages, extending the base message metadata with Storage Queue-specific properties.
/// </summary>
public class StorageQueueMetadata : MessageMetadata
{
    /// <summary>
    /// Gets or sets the number of times this message has been dequeued from the queue.
    /// </summary>
    public long DequeueCount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the message was inserted into the queue.
    /// </summary>
    public DateTimeOffset? InsertedOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the message will expire and be automatically removed from the queue.
    /// </summary>
    public DateTimeOffset? ExpiresOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the message will become visible in the queue for processing.
    /// </summary>
    public DateTimeOffset? NextVisibleOn { get; set; }

    /// <summary>
    /// Gets or sets the pop receipt token that must be provided to complete or abandon the message.
    /// </summary>
    public required string PopReceipt { get; set; }

    /// <summary>
    /// Creates a StorageQueueMetadata instance from an Azure Storage Queue message.
    /// </summary>
    /// <param name="message">The Storage Queue message containing the metadata.</param>
    /// <returns>A new StorageQueueMetadata instance populated with data from the queue message.</returns>
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
