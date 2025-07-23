namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for a service that hosts and manages a message processor.
/// </summary>
/// <typeparam name="TProcessor">The type of message processor.</typeparam>
public interface IMessageProcessorService<out TProcessor>
{
    TProcessor Processor { get; }

    bool IsRunning { get; }

    Task StartAsync(
        CancellationToken cancellationToken);

    Task StopAsync(
        CancellationToken cancellationToken);
}
