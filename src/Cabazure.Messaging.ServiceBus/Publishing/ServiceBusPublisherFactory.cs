using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Publishing;

public class ServiceBusPublisherFactory(
    IOptionsMonitor<CabazureServiceBusOptions> options,
    IEnumerable<ServcieBusPublisherRegistration> registrations,
    IServiceBusSenderProvider senderProvider)
    : IServiceBusPublisherFactory
{
    private readonly Dictionary<Type, ServcieBusPublisherRegistration> publishers
        = registrations.ToDictionary(r => r.Type);

    public IServiceBusPublisher<T> Create<T>(
        string? connectionName = null)
    {
        if (!publishers.TryGetValue(typeof(T), out var publisher))
        {
            throw new ArgumentException(
                $"Type {typeof(T).Name} not configured as a ServiceBus publisher");
        }

        var config = options.Get(connectionName);
        var sender = senderProvider.GetSender<T>(connectionName);

        return new ServiceBusPublisher<T>(
            config.SerializerOptions,
            sender,
            publisher.PropertiesFactory,
            publisher.PartitionKeyFactory);
    }
}
