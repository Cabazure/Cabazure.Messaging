using System.Text.Json;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubConsumerClientFactoryTests
{
    [Theory, AutoNSubstituteData]
    public void Can_Create_Client(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        EventHubConsumerClientFactory sut,
        string connectionName,
        string eventHubName,
        string consumerGroupName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);

        var result = sut.Create(
            connectionName,
            eventHubName,
            consumerGroupName);

        result.Should().BeOfType<EventHubConsumerClientWrapper>();
        result.EventHubName.Should().Be(eventHubName);
        result.ConsumerGroup.Should().Be(consumerGroupName);
        result.FullyQualifiedNamespace.Should().Be(options.FullyQualifiedNamespace);
    }
}
