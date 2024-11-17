using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Tests.Internal;

public class ServiceBusSenderProviderTests
{
    public record TMessage();

    [Theory, AutoNSubstituteData]
    public void GetSender_Throws_If_No_Registration(
        IOptionsMonitor<CabazureServiceBusOptions> monitor,
        IServiceBusClientProvider clientProvider,
        string connectionName)
    {
        var sut = new ServiceBusSenderProvider(
            [],
            clientProvider);

        FluentActions
            .Invoking(() =>
                sut.GetSender<TMessage>(connectionName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Type {nameof(TMessage)} not configured as a ServiceBus publisher");
    }

    [Theory, AutoNSubstituteData]
    public void GetSender_Gets_Client(
        IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        ServiceBusPublisherRegistration registration,
        string connectionName)
    {
        clientProvider.GetClient(default).ReturnsForAnyArgs(client);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new ServiceBusSenderProvider(
            [registration],
            clientProvider);

        sut.GetSender<TMessage>(
            connectionName);

        clientProvider
            .Received(1)
            .GetClient(
                connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void GetSender_Creates_Sender_From_Client(
        IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        ServiceBusPublisherRegistration registration,
        string connectionName)
    {
        clientProvider.GetClient(default).ReturnsForAnyArgs(client);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new ServiceBusSenderProvider(
            [registration],
            clientProvider);

        sut.GetSender<TMessage>(
            connectionName);

        client
            .Received(1)
            .CreateSender(registration.TopicOrQueueName, registration.SenderOptions);
    }


    [Theory, AutoNSubstituteData]
    public void GetSender_Returns_Sender_From_Client(
        IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        [Substitute] ServiceBusSender sender,
        ServiceBusPublisherRegistration registration,
        string connectionName)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);
        client
            .CreateSender(default, default)
            .ReturnsForAnyArgs(sender);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new ServiceBusSenderProvider(
            [registration],
            clientProvider);

        var result = sut.GetSender<TMessage>(
            connectionName);

        result
            .Should()
            .Be(sender);
    }

    [Theory, AutoNSubstituteData]
    public async Task DisposeAsync_Disposes_Senders(
        IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        [Substitute] ServiceBusSender sender,
        ServiceBusPublisherRegistration registration,
        string connectionName)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);
        client
            .CreateSender(default, default)
            .ReturnsForAnyArgs(sender);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new ServiceBusSenderProvider(
            [registration],
            clientProvider);

        sut.GetSender<TMessage>(connectionName);
        await sut.DisposeAsync();

        _ = sender
            .Received(1)
            .DisposeAsync();
    }
}
