using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public interface IServiceBusProcessor
{
    bool IsProcessing { get; }

    event Func<ProcessMessageEventArgs, Task> ProcessMessageAsync;

    event Func<ProcessErrorEventArgs, Task> ProcessErrorAsync;

    Task StartProcessingAsync(CancellationToken cancellationToken);

    Task StopProcessingAsync(CancellationToken cancellationToken);
}

public class ServiceBusProcessorWrapper(
    ServiceBusProcessor processor)
    : IServiceBusProcessor
{
    public ServiceBusProcessor Processor => processor;

    public bool IsProcessing => processor.IsProcessing;

    public event Func<ProcessMessageEventArgs, Task> ProcessMessageAsync
    {
        add => processor.ProcessMessageAsync += value;
        remove => processor.ProcessMessageAsync -= value;
    }

    public event Func<ProcessErrorEventArgs, Task> ProcessErrorAsync
    {
        add => processor.ProcessErrorAsync += value;
        remove => processor.ProcessErrorAsync -= value;
    }

    public Task StartProcessingAsync(CancellationToken cancellationToken)
        => processor.StartProcessingAsync(cancellationToken);

    public Task StopProcessingAsync(CancellationToken cancellationToken)
        => processor.StopProcessingAsync(cancellationToken);
}