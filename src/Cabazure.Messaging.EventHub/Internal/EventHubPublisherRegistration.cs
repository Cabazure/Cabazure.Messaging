using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.Internal;

public record EventHubPublisherRegistration(
    string? ConnectionName,
    Type Type,
    string EventHubName,
    Action<object, EventData>? EventDataModifier,
    Func<object, string>? PartitionKeyFactory);
