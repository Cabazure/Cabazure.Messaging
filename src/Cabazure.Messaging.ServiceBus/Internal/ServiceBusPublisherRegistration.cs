using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public record ServiceBusPublisherRegistration(
    string? ConnectionName,
    Type Type,
    string TopicOrQueueName,
    ServiceBusSenderOptions? SenderOptions,
    Action<object, ServiceBusMessage>? EventDataModifier);
