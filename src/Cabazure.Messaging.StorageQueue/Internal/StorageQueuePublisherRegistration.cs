namespace Cabazure.Messaging.StorageQueue.Internal;

public record StorageQueuePublisherRegistration(
    string? ConnectionName,
    Type Type,
    string QueueName);
