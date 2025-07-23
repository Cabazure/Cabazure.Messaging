namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Defines a factory for creating Service Bus publishers for different message types.
/// </summary>
public interface IServiceBusPublisherFactory
{
    IServiceBusPublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
