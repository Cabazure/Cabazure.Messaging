using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Represents metadata specific to Azure Service Bus messages, extending the base message metadata with Service Bus-specific properties.
/// </summary>
public class ServiceBusMetadata : MessageMetadata
{
    /// <summary>
    /// Gets or sets the description of the error that caused the message to be dead-lettered.
    /// </summary>
    public string? DeadLetterErrorDescription { get; init; }

    /// <summary>
    /// Gets or sets the source that originally enqueued the message before it was dead-lettered.
    /// </summary>
    public string? DeadLetterSource { get; init; }

    /// <summary>
    /// Gets or sets the reason why the message was dead-lettered.
    /// </summary>
    public string? DeadLetterReason { get; init; }

    /// <summary>
    /// Gets or sets the session identifier for the message when using session-aware queues or topics.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Gets or sets the number of times this message has been delivered.
    /// </summary>
    public int DeliveryCount { get; init; }

    /// <summary>
    /// Gets or sets the unique sequence number assigned to the message when it was enqueued.
    /// </summary>
    public long EnqueuedSequenceNumber { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the message expires and will be dead-lettered.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>
    /// Gets or sets the date and time until which the message is locked for processing.
    /// </summary>
    public DateTimeOffset LockedUntil { get; init; }

    /// <summary>
    /// Gets or sets the lock token that can be used to complete or abandon the message.
    /// </summary>
    public string? LockToken { get; init; }

    /// <summary>
    /// Gets or sets the address of an entity to reply to when processing the message.
    /// </summary>
    public string? ReplyTo { get; init; }

    /// <summary>
    /// Gets or sets the session identifier to reply to when processing the message.
    /// </summary>
    public string? ReplyToSessionId { get; init; }

    /// <summary>
    /// Gets or sets the scheduled time when the message should be made available for processing.
    /// </summary>
    public DateTimeOffset ScheduledEnqueueTime { get; init; }

    /// <summary>
    /// Gets or sets the unique sequence number assigned to the message by the message broker.
    /// </summary>
    public long SequenceNumber { get; init; }

    /// <summary>
    /// Gets or sets the current state of the message (Active, Deferred, etc.).
    /// </summary>
    public ServiceBusMessageState State { get; init; }

    /// <summary>
    /// Gets or sets the subject of the message, used for application-defined message classification.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Gets or sets the time-to-live duration for the message.
    /// </summary>
    public TimeSpan TimeToLive { get; init; }

    /// <summary>
    /// Gets or sets the address of an entity that the message is intended for.
    /// </summary>
    public string? To { get; init; }

    /// <summary>
    /// Gets or sets the partition key for transactions that span multiple entities.
    /// </summary>
    public string? TransactionPartitionKey { get; init; }

    /// <summary>
    /// Creates a ServiceBusMetadata instance from an Azure Service Bus received message.
    /// </summary>
    /// <param name="message">The Service Bus received message containing the metadata.</param>
    /// <returns>A new ServiceBusMetadata instance populated with data from the received message.</returns>
    public static ServiceBusMetadata Create(
        ServiceBusReceivedMessage message)
        => new()
        {
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            ContentType = message.ContentType,
            EnqueuedTime = message.EnqueuedTime,
            PartitionKey = message.PartitionKey,
            Properties = message.ApplicationProperties,
            DeadLetterErrorDescription = message.DeadLetterErrorDescription,
            DeadLetterReason = message.DeadLetterReason,
            DeadLetterSource = message.DeadLetterSource,
            DeliveryCount = message.DeliveryCount,
            EnqueuedSequenceNumber = message.EnqueuedSequenceNumber,
            ExpiresAt = message.ExpiresAt,
            LockedUntil = message.LockedUntil,
            LockToken = message.LockToken,
            ReplyTo = message.ReplyTo,
            ReplyToSessionId = message.ReplyToSessionId,
            ScheduledEnqueueTime = message.ScheduledEnqueueTime,
            SequenceNumber = message.SequenceNumber,
            SessionId = message.SessionId,
            State = message.State,
            Subject = message.Subject,
            TimeToLive = message.TimeToLive,
            To = message.To,
            TransactionPartitionKey = message.TransactionPartitionKey,
        };
}
