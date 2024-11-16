namespace Cabazure.Messaging.ServiceBus.Internal;

public record ServiceBusPublisherRegistration(
    string? ConnectionName,
    Type Type,
    string TopicOrQueueName,
    Func<object, Dictionary<string, object>>? PropertiesFactory,
    Func<object, string>? PartitionKeyFactory);
