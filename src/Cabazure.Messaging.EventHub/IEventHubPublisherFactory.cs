namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Defines a factory for creating Event Hub publishers for different message types.
/// </summary>
public interface IEventHubPublisherFactory
{
    /// <summary>
    /// Creates an Event Hub publisher for the specified message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the publisher will handle.</typeparam>
    /// <param name="connectionName">The optional name of the connection configuration to use. If not specified, the default connection will be used.</param>
    /// <returns>An Event Hub publisher instance for the specified message type.</returns>
    IEventHubPublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
