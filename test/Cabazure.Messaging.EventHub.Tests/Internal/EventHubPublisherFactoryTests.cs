using System.Text.Json;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubPublisherFactoryTests
{
    public record TMessage();

    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Registration(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        IOptionsMonitor<CabazureEventHubOptions> monitor,
        IEventHubProducerClientFactory clientFactory,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var sut = new EventHubPublisherFactory(
            monitor,
            [],
            clientFactory);

        FluentActions
            .Invoking(() =>
                sut.Create<TMessage>(connectionName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Type {nameof(TMessage)} not configured as an EventHub publisher");
    }

    [Theory, AutoNSubstituteData]
    public void Create_Creates_Client(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        IOptionsMonitor<CabazureEventHubOptions> monitor,
        IEventHubProducerClientFactory clientFactory,
        EventHubPublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new EventHubPublisherFactory(
            monitor,
            [registration],
            clientFactory);

        sut.Create<TMessage>(
            connectionName);

        clientFactory
            .Received(1)
            .Create(
                connectionName,
                registration.EventHubName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Reads_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        IOptionsMonitor<CabazureEventHubOptions> monitor,
        IEventHubProducerClientFactory clientFactory,
        EventHubPublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new EventHubPublisherFactory(
            monitor,
            [registration],
            clientFactory);

        sut.Create<TMessage>(
            connectionName);

        monitor.Received(1).Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Returns_EventHubPublisher(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        IOptionsMonitor<CabazureEventHubOptions> monitor,
        IEventHubProducerClientFactory clientFactory,
        EventHubPublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new EventHubPublisherFactory(
            monitor,
            [registration],
            clientFactory);

        var publisher = sut.Create<TMessage>(
            connectionName);

        publisher
            .Should()
            .BeOfType<EventHubPublisher<TMessage>>();
    }
}
