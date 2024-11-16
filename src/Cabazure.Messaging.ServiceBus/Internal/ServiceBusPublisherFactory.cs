using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Internal;

public class ServiceBusPublisherFactory(
    IOptionsMonitor<CabazureServiceBusOptions> options,
    IEnumerable<ServiceBusPublisherRegistration> registrations,
    IServiceBusSenderProvider senderProvider)
    : IServiceBusPublisherFactory
{
    private readonly Dictionary<Type, ServiceBusPublisherRegistration> publishers
        = registrations.ToDictionary(r => r.Type);

    public IServiceBusPublisher<TMessage> Create<TMessage>(
        string? connectionName = null)
    {
        if (!publishers.TryGetValue(typeof(TMessage), out var publisher))
        {
            throw new ArgumentException(
                $"Type {typeof(TMessage).Name} not configured as a ServiceBus publisher");
        }

        var config = options.Get(connectionName);
        var sender = senderProvider.GetSender<TMessage>(connectionName);

        return new ServiceBusPublisher<TMessage>(
            config.SerializerOptions,
            sender,
            publisher.PropertiesFactory,
            publisher.PartitionKeyFactory);
    }
}
