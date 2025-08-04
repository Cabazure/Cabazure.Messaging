using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubPublisherFactory(
    IOptionsMonitor<CabazureEventHubOptions> monitor,
    IEnumerable<EventHubPublisherRegistration> registrations,
    IEventHubProducerProvider producerFactory)
    : IEventHubPublisherFactory
{
    private readonly Dictionary<Type, EventHubPublisherRegistration[]> registrations
        = registrations.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.ToArray());

    public IEventHubPublisher<TMessage> Create<TMessage>(
        string? connectionName = null)
    {
        var registration = FindRegistration<TMessage>(connectionName);

        var producer = producerFactory.GetClient(
            connectionName,
            registration.EventHubName);

        var options = monitor.Get(connectionName);
        return new EventHubPublisher<TMessage>(
            options.SerializerOptions,
            producer,
            registration.EventDataModifier,
            registration.PartitionKeyFactory);
    }

    private EventHubPublisherRegistration FindRegistration<TMessage>(
        string? connectionName)
    {
        if (!registrations.TryGetValue(typeof(TMessage), out var matches))
        {
            throw new ArgumentException(
                $"Type {typeof(TMessage).Name} not configured as an EventHub publisher");
        }

        if (matches.Length == 1)
        {
            return matches[0];
        }

        if (matches.FirstOrDefault(m => m.ConnectionName == connectionName) is { } match)
        {
            return match;
        }

        throw new ArgumentException(
            connectionName is { Length: > 0 } c
                ? $"Type {typeof(TMessage).Name} not configured as an EventHub publisher for connection '{c}'"
                : $"Type {typeof(TMessage).Name} not configured as an EventHub publisher for default connection");
    }
}
