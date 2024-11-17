using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

public class EventHubProcessorBuilder
{
    public List<Func<IDictionary<string, object>, bool>> Filters { get; } = [];

    public EventProcessorClientOptions? ProcessorOptions { get; private set; }

    public EventHubProcessorBuilder WithFilter(
        Func<IDictionary<string, object>, bool> predicate)
    {
        Filters.Add(predicate);
        return this;
    }

    public EventHubProcessorBuilder WithClientOptions(
        EventProcessorClientOptions options)
    {
        ProcessorOptions = options;
        return this;
    }
}
