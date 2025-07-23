using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Represents metadata specific to Azure Service Bus messages, extending the base message metadata with Service Bus-specific properties.
/// </summary>
public class ServiceBusMetadata : MessageMetadata
{
    public string? DeadLetterErrorDescription { get; init; }

    public string? DeadLetterSource { get; init; }

    public string? DeadLetterReason { get; init; }

    public string? SessionId { get; init; }

    public int DeliveryCount { get; init; }

    public long EnqueuedSequenceNumber { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset LockedUntil { get; init; }

    public string? LockToken { get; init; }

    public string? ReplyTo { get; init; }

    public string? ReplyToSessionId { get; init; }

    public DateTimeOffset ScheduledEnqueueTime { get; init; }

    public long SequenceNumber { get; init; }

    public ServiceBusMessageState State { get; init; }

    public string? Subject { get; init; }

    public TimeSpan TimeToLive { get; init; }

    public string? To { get; init; }

    public string? TransactionPartitionKey { get; init; }

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
