namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Defines a factory for creating Service Bus publishers for different message types.
/// </summary>
public interface IServiceBusPublisherFactory
{
    /// <summary>
    /// Creates a Service Bus publisher for the specified message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the publisher will handle.</typeparam>
    /// <param name="connectionName">The optional name of the connection configuration to use. If not specified, the default connection will be used.</param>
    /// <returns>A Service Bus publisher instance for the specified message type.</returns>
    IServiceBusPublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
