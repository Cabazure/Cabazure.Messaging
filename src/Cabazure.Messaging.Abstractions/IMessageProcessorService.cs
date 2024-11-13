namespace Cabazure.Messaging;

public interface IMessageProcessorService<out TProcessor>
{
    TProcessor Processor { get; }

    bool IsRunning { get; }

    Task StartAsync(
        CancellationToken cancellationToken);

    Task StopAsync(
        CancellationToken cancellationToken);
}
