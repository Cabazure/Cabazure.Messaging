using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Internal;

public class ServiceBusPublisherFactory(
    IOptionsMonitor<CabazureServiceBusOptions> monitor,
    IEnumerable<ServiceBusPublisherRegistration> registrations,
    IServiceBusSenderProvider senderProvider)
    : IServiceBusPublisherFactory
{
    private readonly Dictionary<Type, ServiceBusPublisherRegistration[]> registrations
        = registrations.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.ToArray());

    public IServiceBusPublisher<TMessage> Create<TMessage>(
        string? connectionName = null)
    {
        var registration = FindRegistration<TMessage>(connectionName);

        var options = monitor.Get(connectionName);
        var sender = senderProvider.GetSender<TMessage>(connectionName);

        return new ServiceBusPublisher<TMessage>(
            options.SerializerOptions,
            sender,
            registration.EventDataModifier);
    }

    private ServiceBusPublisherRegistration FindRegistration<TMessage>(
        string? connectionName)
    {
        if (registrations.TryGetValue(typeof(TMessage), out var matches))
        {
            if (matches.Length == 1)
            {
                return matches[0];
            }

            if (matches.SingleOrDefault(m => m.ConnectionName == connectionName) is { } singleMatch)
            {
                return singleMatch;
            }
        }

        throw new ArgumentException(
            $"Type {typeof(TMessage).Name} not configured as a ServiceBus publisher");
    }
}
