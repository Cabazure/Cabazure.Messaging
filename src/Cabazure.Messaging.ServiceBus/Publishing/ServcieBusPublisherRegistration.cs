namespace Cabazure.Messaging.ServiceBus.Publishing;

public record ServcieBusPublisherRegistration(
    Type Type,
    string TopicName,
    Func<object, Dictionary<string, object>>? PropertiesFactory,
    Func<object, string>? PartitionKeyFactory);
