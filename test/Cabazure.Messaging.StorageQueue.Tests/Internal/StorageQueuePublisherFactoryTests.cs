using System.Text.Json;
using Azure.Storage.Queues;
using Cabazure.Messaging.StorageQueue.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.Tests.Internal;

public class StorageQueuePublisherFactoryTests
{
    public record TMessage();

    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Registration(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        IStorageQueueClientProvider clientFactory,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var sut = new StorageQueuePublisherFactory(
            monitor,
            [],
            clientFactory);

        FluentActions
            .Invoking(() =>
                sut.Create<TMessage>(connectionName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Type {nameof(TMessage)} not configured as a StorageQueue publisher");
    }

    [Theory, AutoNSubstituteData]
    public void Create_Creates_Client(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        IStorageQueueClientProvider clientFactory,
        [Substitute] QueueServiceClient client,
        StorageQueuePublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        clientFactory.GetClient(default).ReturnsForAnyArgs(client);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new StorageQueuePublisherFactory(
            monitor,
            [registration],
            clientFactory);

        sut.Create<TMessage>(
            connectionName);

        clientFactory
            .Received(1)
            .GetClient(
                connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Reads_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        IStorageQueueClientProvider clientFactory,
        [Substitute] QueueServiceClient client,
        StorageQueuePublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        clientFactory.GetClient(default).ReturnsForAnyArgs(client);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new StorageQueuePublisherFactory(
            monitor,
            [registration],
            clientFactory);

        sut.Create<TMessage>(
            connectionName);

        monitor.Received(1).Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Returns_StorageQueuePublisher(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        IStorageQueueClientProvider clientFactory,
        [Substitute] QueueServiceClient client,
        StorageQueuePublisherRegistration registration,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        clientFactory.GetClient(default).ReturnsForAnyArgs(client);
        registration = registration with
        {
            ConnectionName = connectionName,
            Type = typeof(TMessage),
        };
        var sut = new StorageQueuePublisherFactory(
            monitor,
            [registration],
            clientFactory);

        var publisher = sut.Create<TMessage>(
            connectionName);

        publisher
            .Should()
            .BeOfType<StorageQueuePublisher<TMessage>>();
    }
}
