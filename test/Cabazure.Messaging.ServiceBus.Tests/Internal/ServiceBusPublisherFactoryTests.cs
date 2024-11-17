using System.Text.Json;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Tests.Internal;

public class ServiceBusPublisherFactoryTests
{
    public record TMessage();

    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Registration(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        IOptionsMonitor<CabazureServiceBusOptions> monitor,
        IServiceBusSenderProvider senderProvider,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var sut = new ServiceBusPublisherFactory(
            monitor,
            [],
            senderProvider);

        FluentActions
            .Invoking(() =>
                sut.Create<TMessage>(connectionName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Type {nameof(TMessage)} not configured as a ServiceBus publisher");
    }

    [Theory, AutoNSubstituteData]
    public void Create_Gets_Sender(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        IOptionsMonitor<CabazureServiceBusOptions> monitor,
        IServiceBusSenderProvider senderProvider,
        ServiceBusPublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new ServiceBusPublisherFactory(
            monitor,
            [registration],
            senderProvider);

        sut.Create<TMessage>(
            connectionName);

        senderProvider
            .Received(1)
            .GetSender<TMessage>(
                connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Reads_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        IOptionsMonitor<CabazureServiceBusOptions> monitor,
        IServiceBusSenderProvider senderProvider,
        ServiceBusPublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new ServiceBusPublisherFactory(
            monitor,
            [registration],
            senderProvider);

        sut.Create<TMessage>(
            connectionName);

        monitor.Received(1).Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Returns_ServiceBusPublisher(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        IOptionsMonitor<CabazureServiceBusOptions> monitor,
        IServiceBusSenderProvider senderProvider,
        ServiceBusPublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new ServiceBusPublisherFactory(
            monitor,
            [registration],
            senderProvider);

        var publisher = sut.Create<TMessage>(
            connectionName);

        publisher
            .Should()
            .BeOfType<ServiceBusPublisher<TMessage>>();
    }
}
