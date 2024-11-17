using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.DependencyInjection;

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
