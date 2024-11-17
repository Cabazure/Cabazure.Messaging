using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;

namespace Cabazure.Messaging.ServiceBus.Tests.Internal;

public class ServiceBusProcessorFactoryTests
{
    [Theory, AutoNSubstituteData]
    public void Create_For_Topic_Gets_Client(
        [Frozen] IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        ServiceBusProcessorFactory sut,
        string connectionName,
        string topicName,
        string consumerGroupName,
        ServiceBusProcessorOptions options)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);

        sut.Create(
            connectionName,
            topicName,
            consumerGroupName,
            options);

        clientProvider
            .Received(1)
            .GetClient(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_For_Topic_Creates_Processor(
        [Frozen] IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        ServiceBusProcessorFactory sut,
        string connectionName,
        string topicName,
        string consumerGroupName,
        ServiceBusProcessorOptions options)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);

        sut.Create(
            connectionName,
            topicName,
            consumerGroupName,
            options);

        client
            .Received(1)
            .CreateProcessor(
                topicName,
                consumerGroupName,
                options);
    }

    [Theory, AutoNSubstituteData]
    public void Create_For_Topic_Returns_Processor(
        [Frozen] IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        [Substitute] ServiceBusProcessor processor,
        ServiceBusProcessorFactory sut,
        string connectionName,
        string topicName,
        string consumerGroupName,
        ServiceBusProcessorOptions options)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);
        client
            .CreateProcessor(default, default, default)
            .ReturnsForAnyArgs(processor);

        var result = sut.Create(
            connectionName,
            topicName,
            consumerGroupName,
            options);

        result
            .Should()
            .Be(processor);
    }

    [Theory, AutoNSubstituteData]
    public void Create_For_Queue_Gets_Client(
        [Frozen] IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        ServiceBusProcessorFactory sut,
        string connectionName,
        string queueName,
        ServiceBusProcessorOptions options)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);

        sut.Create(
            connectionName,
            queueName,
            options);

        clientProvider
            .Received(1)
            .GetClient(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_For_Queue_Creates_Processor(
        [Frozen] IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        ServiceBusProcessorFactory sut,
        string connectionName,
        string queueName,
        ServiceBusProcessorOptions options)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);

        sut.Create(
            connectionName,
            queueName,
            options);

        client
            .Received(1)
            .CreateProcessor(
                queueName,
                options);
    }

    [Theory, AutoNSubstituteData]
    public void Create_For_Queue_Returns_Processor(
        [Frozen] IServiceBusClientProvider clientProvider,
        [Substitute] ServiceBusClient client,
        [Substitute] ServiceBusProcessor processor,
        ServiceBusProcessorFactory sut,
        string connectionName,
        string queueName,
        ServiceBusProcessorOptions options)
    {
        clientProvider
            .GetClient(default)
            .ReturnsForAnyArgs(client);
        client
            .CreateProcessor(default, default(ServiceBusProcessorOptions))
            .ReturnsForAnyArgs(processor);

        var result = sut.Create(
            connectionName,
            queueName,
            options);

        result
            .Should()
            .Be(processor);
    }
}
