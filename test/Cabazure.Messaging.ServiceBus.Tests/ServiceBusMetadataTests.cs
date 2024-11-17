using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Tests;

public class ServiceBusMetadataTests
{
    [Theory, AutoNSubstituteData]
    public void Create_Should_Map_Properties_Correctly(
        BinaryData body,
        string messageId,
        string partitionKey,
        string viaPartitionKey,
        string sessionId,
        string replyToSessionId,
        TimeSpan timeToLive,
        string correlationId,
        string subject,
        string to,
        string contentType,
        string replyTo,
        DateTimeOffset scheduledEnqueueTime,
        IDictionary<string, object> properties,
        Guid lockTokenGuid,
        int deliveryCount,
        DateTimeOffset lockedUntil,
        long sequenceNumber,
        string deadLetterSource,
        long enqueuedSequenceNumber,
        DateTimeOffset enqueuedTime,
        ServiceBusMessageState serviceBusMessageState)
    {
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: null,
            messageId: messageId,
            partitionKey: partitionKey,
            viaPartitionKey: viaPartitionKey,
            sessionId: sessionId,
            replyToSessionId: replyToSessionId,
            timeToLive: timeToLive,
            correlationId: correlationId,
            subject: subject,
            to: to,
            contentType: contentType,
            replyTo: replyTo,
            scheduledEnqueueTime: scheduledEnqueueTime,
            properties: properties,
            lockTokenGuid: lockTokenGuid,
            deliveryCount: deliveryCount,
            lockedUntil: lockedUntil,
            sequenceNumber: sequenceNumber,
            deadLetterSource: deadLetterSource,
            enqueuedSequenceNumber: enqueuedSequenceNumber,
            enqueuedTime: enqueuedTime,
            serviceBusMessageState: serviceBusMessageState);

        ServiceBusMetadata
            .Create(message)
            .Should()
            .BeEquivalentTo(new ServiceBusMetadata
            {
                CorrelationId = message.CorrelationId,
                ContentType = message.ContentType,
                DeadLetterErrorDescription = message.DeadLetterErrorDescription,
                DeadLetterReason = message.DeadLetterReason,
                DeadLetterSource = message.DeadLetterSource,
                DeliveryCount = message.DeliveryCount,
                EnqueuedTime = message.EnqueuedTime,
                EnqueuedSequenceNumber = message.EnqueuedSequenceNumber,
                ExpiresAt = message.ExpiresAt,
                LockedUntil = message.LockedUntil,
                LockToken = message.LockToken,
                MessageId = message.MessageId,
                PartitionKey = message.PartitionKey,
                Properties = message.ApplicationProperties,
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
            });
    }
}
