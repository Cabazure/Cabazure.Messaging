using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Service Bus processors with options such as filters and processor settings.
/// </summary>
public class ServiceBusProcessorBuilder
{
    public List<Func<IReadOnlyDictionary<string, object>, bool>> Filters { get; } = [];

    public ServiceBusProcessorOptions? ProcessorOptions { get; private set; }

    public ServiceBusProcessorBuilder WithFilter(
        Func<IReadOnlyDictionary<string, object>, bool> predicate)
    {
        Filters.Add(predicate);
        return this;
    }

    public ServiceBusProcessorBuilder WithProcessorOptions(
        ServiceBusProcessorOptions options)
    {
        ProcessorOptions = options;
        return this;
    }
}
