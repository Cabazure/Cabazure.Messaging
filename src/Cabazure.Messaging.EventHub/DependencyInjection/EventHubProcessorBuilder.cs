using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

public class EventHubProcessorBuilder
{
    public List<Func<IDictionary<string, object>, bool>> Filters { get; } = [];

    public EventProcessorClientOptions? ClientOptions { get; private set; }

    public EventHubProcessorBuilder WithFilter(
        Func<IDictionary<string, object>, bool> predicate)
    {
        Filters.Add(predicate);
        return this;
    }

    public EventHubProcessorBuilder WithClientOptions(
        EventProcessorClientOptions options)
    {
        ClientOptions = options;
        return this;
    }
}
