using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubPublisherFactory(
    IOptionsMonitor<CabazureEventHubOptions> monitor,
    IEnumerable<EventHubPublisherRegistration> registrations,
    IEventHubProducerProvider producerFactory)
    : IEventHubPublisherFactory
{
    private sealed record PublisherKey(string? Connection, Type Type);
    private readonly Dictionary<PublisherKey, EventHubPublisherRegistration> publishers
        = registrations.ToDictionary(r => new PublisherKey(r.ConnectionName, r.Type));

    public IEventHubPublisher<TMessage> Create<TMessage>(
        string? connectionName = null)
    {
        var key = new PublisherKey(connectionName, typeof(TMessage));
        if (!publishers.TryGetValue(key, out var publisher))
        {
            throw new ArgumentException(
                $"Type {typeof(TMessage).Name} not configured as an EventHub publisher");
        }

        var producer = producerFactory.GetClient(
            connectionName,
            publisher.EventHubName);

        var options = monitor.Get(connectionName);
        return new EventHubPublisher<TMessage>(
            options.SerializerOptions,
            producer,
            publisher.EventDataModifier,
            publisher.PartitionKeyFactory);
    }
}
