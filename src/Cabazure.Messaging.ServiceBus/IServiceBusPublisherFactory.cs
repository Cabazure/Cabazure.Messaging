namespace Cabazure.Messaging.ServiceBus;

public interface IServiceBusPublisherFactory
{
    IServiceBusPublisher<T> Create<T>(
        string? connectionName = null);
}
