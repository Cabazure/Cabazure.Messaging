using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.DependencyInjection;

namespace Cabazure.Messaging.ServiceBus.Tests.DependencyInjection;

public class ServiceBusProcessorBuilderTests
{
    [Theory, AutoNSubstituteData]
    public void WithFilter_Adds_Filter(
        ServiceBusProcessorBuilder sut,
        Func<IReadOnlyDictionary<string, object>, bool> filter)
    {
        sut.WithFilter(filter);

        sut.Filters.Contains(filter);
    }

    [Theory, AutoNSubstituteData]
    public void WithProcessorOptions_Sets_ProcessorOptions(
        ServiceBusProcessorBuilder sut,
        [NoAutoProperties] ServiceBusProcessorOptions options)
    {
        sut.WithProcessorOptions(options);

        sut.ProcessorOptions.Should().Be(options);
    }
}
