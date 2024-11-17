using System.Text.Json;
using Cabazure.Messaging.EventHub.DependencyInjection;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.DependencyInjection;

public class EventHubBuilderTests
{
    public record TMessage();
    public class TConfigureOptions : IConfigureOptions<CabazureEventHubOptions>
    {
        public void Configure(CabazureEventHubOptions options) => throw new NotImplementedException();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Expose_Services_Add_ConnectionName(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        EventHubBuilder sut)
    {
        sut.Services
            .Should()
            .BeSameAs(services);
        sut.ConnectionName
            .Should()
            .Be(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Configure_Should_Configure_CabazureEventHubOptions(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        EventHubBuilder sut,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options)
    {
        sut.Configure(o =>
        {
            o.BlobStorage = options.BlobStorage;
            o.ConnectionString = options.ConnectionString;
            o.Credential = options.Credential;
            o.FullyQualifiedNamespace = options.FullyQualifiedNamespace;
            o.SerializerOptions = options.SerializerOptions;
        });

        services
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<CabazureEventHubOptions>>()
            .Get(connectionName)
            .Should()
            .BeEquivalentTo(options);
    }

    [Theory, AutoNSubstituteData]
    public void Configure_Should_Configure_CabazureEventHubOptions_Using_ConfigureOptions(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        EventHubBuilder sut)
    {
        sut.Configure<TConfigureOptions>();

        services
            .Should()
            .Contain<IConfigureOptions<CabazureEventHubOptions>, TConfigureOptions>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_Dependencies(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        EventHubBuilder sut)
    {
        sut.AddPublisher<TMessage>(eventHubName);

        services
            .Should()
            .Contain<IEventHubPublisherFactory, EventHubPublisherFactory>()
            .And.Contain<IEventHubProducerClientProvider, EventHubProducerClientProvider>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Calls_Builder(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        [Substitute] Action<EventHubPublisherBuilder<TMessage>> builder,
        EventHubBuilder sut)
    {
        sut.AddPublisher(eventHubName, builder);

        builder
            .Received(1)
            .Invoke(Arg.Any<EventHubPublisherBuilder<TMessage>>());
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_PublisherRegistration(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        EventHubBuilder sut)
    {
        sut.AddPublisher<TMessage>(eventHubName);

        var registration = (EventHubPublisherRegistration)services
            .First(d => d.ServiceType == typeof(EventHubPublisherRegistration))
            .ImplementationInstance!;

        registration
            .Should()
            .BeEquivalentTo(
                new EventHubPublisherRegistration(
                    connectionName,
                    typeof(TMessage),
                    eventHubName,
                    null,
                    null));
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_EventHubPublisher_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string eventHubName,
        EventHubBuilder sut,
        IEventHubPublisherFactory factory,
        IEventHubPublisher<TMessage> publisher)
    {
        services.AddSingleton(factory);
        factory
            .Create<TMessage>()
            .Returns(publisher);

        sut.AddPublisher<TMessage>(eventHubName);

        services
            .BuildServiceProvider()
            .GetRequiredService<IEventHubPublisher<TMessage>>();
        factory
            .Received(1)
            .Create<TMessage>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_MessagePublisher_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string eventHubName,
        EventHubBuilder sut,
        IEventHubPublisherFactory factory,
        IEventHubPublisher<TMessage> publisher)
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
