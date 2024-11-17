using System.Text.Json;
using Cabazure.Messaging.ServiceBus.DependencyInjection;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Tests.DependencyInjection;

public class ServiceBusBuilderTests
{
    public record TMessage();
    public class TConfigureOptions : IConfigureOptions<CabazureServiceBusOptions>
    {
        public void Configure(CabazureServiceBusOptions options) => throw new NotImplementedException();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Expose_Services_Add_ConnectionName(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        ServiceBusBuilder sut)
    {
        sut.Services
            .Should()
            .BeSameAs(services);
        sut.ConnectionName
            .Should()
            .Be(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Configure_Should_Configure_CabazureServiceBusOptions(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        ServiceBusBuilder sut,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options)
    {
        sut.Configure(o =>
        {
            o.ConnectionString = options.ConnectionString;
            o.Credential = options.Credential;
            o.FullyQualifiedNamespace = options.FullyQualifiedNamespace;
            o.SerializerOptions = options.SerializerOptions;
        });

        services
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<CabazureServiceBusOptions>>()
            .Get(connectionName)
            .Should()
            .BeEquivalentTo(options);
    }

    [Theory, AutoNSubstituteData]
    public void Configure_Should_Configure_CabazureServiceBusOptions_Using_ConfigureOptions(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        ServiceBusBuilder sut)
    {
        sut.Configure<TConfigureOptions>();

        services
            .Should()
            .Contain<IConfigureOptions<CabazureServiceBusOptions>, TConfigureOptions>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_Dependencies(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        ServiceBusBuilder sut)
    {
        sut.AddPublisher<TMessage>(eventHubName);

        services
            .Should()
            .Contain<IServiceBusPublisherFactory, ServiceBusPublisherFactory>()
            .And.Contain<IServiceBusClientProvider, ServiceBusClientProvider>()
            .And.Contain<IServiceBusSenderProvider, ServiceBusSenderProvider>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Calls_Builder(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        [Substitute] Action<ServiceBusPublisherBuilder<TMessage>> builder,
        ServiceBusBuilder sut)
    {
        sut.AddPublisher(eventHubName, builder);

        builder
            .Received(1)
            .Invoke(Arg.Any<ServiceBusPublisherBuilder<TMessage>>());
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_PublisherRegistration(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        ServiceBusBuilder sut)
    {
        sut.AddPublisher<TMessage>(eventHubName);

        var registration = (ServiceBusPublisherRegistration)services
            .First(d => d.ServiceType == typeof(ServiceBusPublisherRegistration))
            .ImplementationInstance!;

        registration
            .Should()
            .BeEquivalentTo(
                new ServiceBusPublisherRegistration(
                    connectionName,
                    typeof(TMessage),
                    eventHubName,
                    null,
                    null));
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_ServiceBusPublisher_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string eventHubName,
        ServiceBusBuilder sut,
        IServiceBusPublisherFactory factory,
        IServiceBusPublisher<TMessage> publisher)
    {
        services.AddSingleton(factory);
        factory
            .Create<TMessage>()
            .Returns(publisher);

        sut.AddPublisher<TMessage>(eventHubName);

        services
            .BuildServiceProvider()
            .GetRequiredService<IServiceBusPublisher<TMessage>>();
        factory
            .Received(1)
            .Create<TMessage>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_MessagePublisher_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string eventHubName,
        ServiceBusBuilder sut,
        IServiceBusPublisherFactory factory,
        IServiceBusPublisher<TMessage> publisher)
    {
        services.AddSingleton(factory);
        factory
            .Create<TMessage>()
            .Returns(publisher);

        sut.AddPublisher<TMessage>(eventHubName);

        services
            .BuildServiceProvider()
            .GetRequiredService<IMessagePublisher<TMessage>>();
        factory
            .Received(1)
            .Create<TMessage>();
    }
}
