using System.Text.Json;
using Azure.Messaging.EventHubs.Primitives;
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
    public class TProcessor : IMessageProcessor<TMessage>
    {
        public virtual Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken) => Task.CompletedTask;
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
            .And.Contain<IEventHubProducerProvider, EventHubProducerProvider>();
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

    [Theory, AutoNSubstituteData]
    public void AddProcessor_Registers_Dependencies(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        EventHubBuilder sut)
    {
        sut.AddProcessor<TMessage, TProcessor>(eventHubName);

        services
            .Should()
            .Contain<IBlobStorageClientProvider, BlobStorageClientProvider>()
            .And.Contain<IEventHubBatchProcessorFactory, EventHubBatchProcessorFactory>()
            .And.Contain<TProcessor, TProcessor>();
    }

    [Theory, AutoNSubstituteData]
    public void AddProcessor_Calls_Builder(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string eventHubName,
        string consumerGroupName,
        [Substitute] Action<EventHubProcessorBuilder> builder,
        EventHubBuilder sut)
    {
        sut.AddProcessor<TMessage, TProcessor>(
            eventHubName,
            consumerGroupName,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Any<EventHubProcessorBuilder>());
    }

    [Theory, AutoNSubstituteData]
    public void AddProcessor_With_Default_BlobContainer_Registers_EventHubBatchProcessor_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string eventHubName,
        string consumerGroupName,
        EventHubBuilder sut,
        IEventHubBatchProcessorFactory factory,
        IEventHubBatchProcessor<TProcessor> batchProcessor,
        [Substitute] TProcessor processor)
    {
        factory
            .Create<TMessage, TProcessor>(default, default, default, default, default, default, default)
            .ReturnsForAnyArgs(batchProcessor);
        services.AddOptions<CabazureEventHubOptions>();
        services.AddSingleton(factory);
        services.AddSingleton(processor);

        sut.AddProcessor<TMessage, TProcessor>(
            eventHubName,
            consumerGroupName);

        var result = services
            .BuildServiceProvider()
            .GetRequiredService<EventHubBatchProcessor<TMessage, TProcessor>>();
        result
            .Should()
            .Be(batchProcessor);

        factory
            .Received(1)
            .Create<TMessage, TProcessor>(
                processor: Arg.Any<TProcessor>(),
                connectionName: sut.ConnectionName,
                eventHubName: eventHubName,
                consumerGroup: consumerGroupName,
                filters: Arg.Is<List<Func<IDictionary<string, object>, bool>>>(l => l.Count == 0),
                containerOptions: new BlobContainerOptions(
                    ContainerName: eventHubName,
                    CreateIfNotExist: true),
                processorOptions: null);
    }

    [Theory, AutoNSubstituteData]
    public void AddProcessor_With_BlobContainerOptions_Registers_EventHubBatchProcessor_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string eventHubName,
        string consumerGroupName,
        BlobContainerOptions containerOptions,
        EventHubBuilder sut,
        IEventHubBatchProcessorFactory factory,
        IEventHubBatchProcessor<TProcessor> batchProcessor,
        [Substitute] TProcessor processor)
    {
        factory
            .Create<TMessage, TProcessor>(default, default, default, default, default, default, default)
            .ReturnsForAnyArgs(batchProcessor);
        services.AddOptions<CabazureEventHubOptions>();
        services.AddSingleton(factory);
        services.AddSingleton(processor);

        sut.AddProcessor<TMessage, TProcessor>(
            eventHubName,
            consumerGroupName,
            b => b.WithBlobContainer(
                containerOptions.ContainerName,
                containerOptions.CreateIfNotExist));

        var result = services
            .BuildServiceProvider()
            .GetRequiredService<EventHubBatchProcessor<TMessage, TProcessor>>();
        result
            .Should()
            .Be(batchProcessor);

        factory
            .Received(1)
            .Create<TMessage, TProcessor>(
                processor: processor,
                connectionName: sut.ConnectionName,
                eventHubName: eventHubName,
                consumerGroup: consumerGroupName,
                filters: Arg.Is<List<Func<IDictionary<string, object>, bool>>>(l => l.Count == 0),
                containerOptions: containerOptions,
                processorOptions: null);
    }

    [Theory, AutoNSubstituteData]
    public void AddProcessor_With_EventprocessorOptions_Registers_EventHubBatchProcessor_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string eventHubName,
        string consumerGroupName,
        EventProcessorOptions processorOptions,
        EventHubBuilder sut,
        IEventHubBatchProcessorFactory factory,
        IEventHubBatchProcessor<TProcessor> batchProcessor,
        [Substitute] TProcessor processor)
    {
        factory
            .Create<TMessage, TProcessor>(default, default, default, default, default, default, default)
            .ReturnsForAnyArgs(batchProcessor);
        services.AddOptions<CabazureEventHubOptions>();
        services.AddSingleton(factory);
        services.AddSingleton(processor);

        sut.AddProcessor<TMessage, TProcessor>(
            eventHubName,
            consumerGroupName,
            b => b
                .WithProcessorOptions(processorOptions));

        var result = services
            .BuildServiceProvider()
            .GetRequiredService<EventHubBatchProcessor<TMessage, TProcessor>>();
        result.Processor
            .Should()
            .Be(batchProcessor.Processor);

        factory
            .Received(1)
            .Create<TMessage, TProcessor>(
                processor: Arg.Any<TProcessor>(),
                connectionName: sut.ConnectionName,
                eventHubName: eventHubName,
                consumerGroup: consumerGroupName,
                filters: Arg.Is<List<Func<IDictionary<string, object>, bool>>>(l => l.Count == 0),
                containerOptions: Arg.Any<BlobContainerOptions>(),
                processorOptions: processorOptions);
    }
}
