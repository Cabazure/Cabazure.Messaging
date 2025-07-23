namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Defines a factory for creating Storage Queue publishers for different message types.
/// </summary>
public interface IStorageQueuePublisherFactory
{
    /// <summary>
    /// Creates a Storage Queue publisher for the specified message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the publisher will handle.</typeparam>
    /// <param name="connectionName">The optional name of the connection configuration to use. If not specified, the default connection will be used.</param>
    /// <returns>A Storage Queue publisher instance for the specified message type.</returns>
    IStorageQueuePublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
