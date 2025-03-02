using System.Text.Json;
using Azure.Storage.Queues;
using Cabazure.Messaging.StorageQueue.DependencyInjection;
using Cabazure.Messaging.StorageQueue.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.Tests.DependencyInjection;

public class StorageQueueBuilderTests
{
    public record TMessage();
    public class TConfigureOptions : IConfigureOptions<CabazureStorageQueueOptions>
    {
        public void Configure(CabazureStorageQueueOptions options) => throw new NotImplementedException();
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
        StorageQueueBuilder sut)
    {
        sut.Services
            .Should()
            .BeSameAs(services);
        sut.ConnectionName
            .Should()
            .Be(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Configure_Should_Configure_CabazureStorageQueueOptions(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        StorageQueueBuilder sut,
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options)
    {
        sut.Configure(o =>
        {
            o.ConnectionString = options.ConnectionString;
            o.Credential = options.Credential;
            o.QueueServiceUri = options.QueueServiceUri;
            o.SerializerOptions = options.SerializerOptions;
        });

        services
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<CabazureStorageQueueOptions>>()
            .Get(connectionName)
            .Should()
            .BeEquivalentTo(options);
    }

    [Theory, AutoNSubstituteData]
    public void Configure_Should_Configure_CabazureStorageQueueOptions_Using_ConfigureOptions(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        StorageQueueBuilder sut)
    {
        sut.Configure<TConfigureOptions>();

        services
            .Should()
            .Contain<IConfigureOptions<CabazureStorageQueueOptions>, TConfigureOptions>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_Dependencies(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string queueName,
        StorageQueueBuilder sut)
    {
        sut.AddPublisher<TMessage>(queueName);

        services
            .Should()
            .Contain<IStorageQueuePublisherFactory, StorageQueuePublisherFactory>()
            .And.Contain<IStorageQueueClientProvider, StorageQueueClientProvider>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_PublisherRegistration(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string queueName,
        StorageQueueBuilder sut)
    {
        sut.AddPublisher<TMessage>(queueName);

        var registration = (StorageQueuePublisherRegistration)services
            .First(d => d.ServiceType == typeof(StorageQueuePublisherRegistration))
            .ImplementationInstance!;

        registration
            .Should()
            .BeEquivalentTo(
                new StorageQueuePublisherRegistration(
                    connectionName,
                    typeof(TMessage),
                    queueName));
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_StorageQueuePublisher_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string queueName,
        StorageQueueBuilder sut,
        IStorageQueuePublisherFactory factory,
        IStorageQueuePublisher<TMessage> publisher)
    {
        services.AddSingleton(factory);
        factory
            .Create<TMessage>()
            .Returns(publisher);

        sut.AddPublisher<TMessage>(queueName);

        services
            .BuildServiceProvider()
            .GetRequiredService<IStorageQueuePublisher<TMessage>>();
        factory
            .Received(1)
            .Create<TMessage>();
    }

    [Theory, AutoNSubstituteData]
    public void AddPublisher_Registers_MessagePublisher_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string queueName,
        StorageQueueBuilder sut,
        IStorageQueuePublisherFactory factory,
        IStorageQueuePublisher<TMessage> publisher)
    {
        services.AddSingleton(factory);
        factory
            .Create<TMessage>()
            .Returns(publisher);

        sut.AddPublisher<TMessage>(queueName);

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
        string queueName,
        StorageQueueBuilder sut)
    {
        sut.AddProcessor<TMessage, TProcessor>(queueName);

        services
            .Should()
            .Contain<IStorageQueueClientProvider, StorageQueueClientProvider>()
            .And.Contain<TProcessor, TProcessor>();
    }

    [Theory, AutoNSubstituteData]
    public void AddProcessor_Calls_Builder(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        [Frozen(Matching.ParameterName)]
        string connectionName,
        string queueName,
        [Substitute] Action<StorageQueueProcessorBuilder> builder,
        StorageQueueBuilder sut)
    {
        sut.AddProcessor<TMessage, TProcessor>(
            queueName,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Any<StorageQueueProcessorBuilder>());
    }

    [Theory, AutoNSubstituteData]
    public void AddProcessor_With_Default_BlobContainer_Registers_StorageQueueBatchProcessor_Using_Factory(
        [Frozen(Matching.ImplementedInterfaces)]
        ServiceCollection services,
        string queueName,
        StorageQueueBuilder sut,
        IStorageQueueClientProvider clientProvider,
        [Substitute] QueueServiceClient client,
        [Substitute] QueueClient queue,
        [Substitute] TProcessor processor)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);
        client.GetQueueClient(default)
            .ReturnsForAnyArgs(queue);
        services.AddOptions<CabazureStorageQueueOptions>();
        services.AddSingleton(clientProvider);
        services.AddSingleton(processor);

        sut.AddProcessor<TMessage, TProcessor>(
            queueName);

        var result = services
            .BuildServiceProvider()
            .GetRequiredService<StorageQueueProcessorService<TMessage, TProcessor>>();

        clientProvider
            .Received(1)
            .GetClient(sut.ConnectionName);
        client
            .Received(1)
            .GetQueueClient(queueName);
    }
}
