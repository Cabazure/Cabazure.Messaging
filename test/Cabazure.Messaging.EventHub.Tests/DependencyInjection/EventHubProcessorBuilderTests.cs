using Azure.Messaging.EventHubs.Primitives;
using Cabazure.Messaging.EventHub.DependencyInjection;

namespace Cabazure.Messaging.EventHub.Tests.DependencyInjection;

public class EventHubProcessorBuilderTests
{
    [Theory, AutoNSubstituteData]
    public void WithFilter_Adds_Filter(
        EventHubProcessorBuilder sut,
        Func<IDictionary<string, object>, bool> filter)
    {
        sut.WithFilter(filter);

        sut.Filters.Contains(filter);
    }

    [Theory, AutoNSubstituteData]
    public void WithProcessorOptions_Sets_ProcessorOptions(
        EventHubProcessorBuilder sut,
        [NoAutoProperties] EventProcessorOptions options)
    {
        sut.WithProcessorOptions(options);

        sut.ProcessorOptions.Should().Be(options);
    }
}
