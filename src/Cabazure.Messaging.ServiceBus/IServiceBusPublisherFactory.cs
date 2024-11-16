namespace Cabazure.Messaging.ServiceBus;

public interface IServiceBusPublisherFactory
{
    IServiceBusPublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
