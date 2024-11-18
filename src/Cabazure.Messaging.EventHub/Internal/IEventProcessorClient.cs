using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventProcessorClient
{
    bool IsRunning { get; }

    string FullyQualifiedNamespace { get; }

    string EventHubName { get; }

    string ConsumerGroup { get; }

    event Func<ProcessEventArgs, Task> ProcessEventAsync;

    event Func<ProcessErrorEventArgs, Task> ProcessErrorAsync;

    Task StartProcessingAsync(CancellationToken cancellationToken = default);

    Task StopProcessingAsync(CancellationToken cancellationToken = default);
}

public class EventProcessorClientWrapper(
    EventProcessorClient client)
    : IEventProcessorClient
{
    public bool IsRunning => client.IsRunning;

    public string FullyQualifiedNamespace => client.FullyQualifiedNamespace;

    public string EventHubName => client.EventHubName;

    public string ConsumerGroup => client.ConsumerGroup;

    public event Func<ProcessEventArgs, Task> ProcessEventAsync
    {
        add => client.ProcessEventAsync += value;
        remove => client.ProcessEventAsync -= value;
    }

    public event Func<ProcessErrorEventArgs, Task> ProcessErrorAsync
    {
        add => client.ProcessErrorAsync += value;
        remove => client.ProcessErrorAsync -= value;
    }

    public Task StartProcessingAsync(CancellationToken cancellationToken = default)
        => client.StartProcessingAsync(cancellationToken);

    public Task StopProcessingAsync(CancellationToken cancellationToken = default)
        => client.StopProcessingAsync(cancellationToken);
}
