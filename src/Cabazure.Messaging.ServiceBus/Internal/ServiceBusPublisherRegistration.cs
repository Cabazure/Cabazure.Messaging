using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public record ServiceBusPublisherRegistration(
    string? ConnectionName,
    Type Type,
    string TopicOrQueueName,
    ServiceBusSenderOptions? SenderOptions,
    Func<object, Dictionary<string, object>>? PropertiesFactory,
    Func<object, string>? PartitionKeyFactory);
