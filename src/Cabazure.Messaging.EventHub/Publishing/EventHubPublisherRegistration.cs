namespace Cabazure.Messaging.EventHub.Publishing;

public record EventHubPublisherRegistration(
    string? ConnectionName,
    Type Type,
    string EventHubName,
    Func<object, Dictionary<string, object>>? PropertiesFactory,
    Func<object, string>? PartitionKeyFactory);
