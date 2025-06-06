﻿using Azure.Messaging.EventHubs.Consumer;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

public class EventHubStatelessProcessorBuilder
{
    public List<Func<IDictionary<string, object>, bool>> Filters { get; } = [];

    public ReadEventOptions? ReadOptions { get; private set; }

    public EventHubStatelessProcessorBuilder WithFilter(
        Func<IDictionary<string, object>, bool> predicate)
    {
        Filters.Add(predicate);
        return this;
    }

    public EventHubStatelessProcessorBuilder WithReadEventOptions(
        ReadEventOptions options)
    {
        ReadOptions = options;
        return this;
    }
}
