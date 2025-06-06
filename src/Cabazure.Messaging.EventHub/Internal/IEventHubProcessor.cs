﻿namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubProcessor<TProcessor>
{
    string FullyQualifiedNamespace { get; }

    string EventHubName { get; }

    string ConsumerGroup { get; }

    TProcessor Processor { get; }

    bool IsRunning { get; }

    Task StartProcessingAsync(
        CancellationToken cancellationToken);

    Task StopProcessingAsync(
        CancellationToken cancellationToken);
}
