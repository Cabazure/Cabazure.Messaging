using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public interface IServiceBusProcessorFactory
{
    IServiceBusProcessor Create(
        string? connectionName,
        string queueName,
        ServiceBusProcessorOptions? options = null);

    IServiceBusProcessor Create(
        string? connectionName,
        string topicName,
        string subscriptionName,
        ServiceBusProcessorOptions? options = null);
}

public class ServiceBusProcessorFactory(
    IServiceBusClientProvider clientProvider)
    : IServiceBusProcessorFactory
{
    public IServiceBusProcessor Create(
        string? connectionName,
        string topicName,
        string subscriptionName,
        ServiceBusProcessorOptions? options = null)
        => new ServiceBusProcessorWrapper(
            clientProvider
                .GetClient(connectionName)
                .CreateProcessor(
                    topicName,
                    subscriptionName,
                    options));

    public IServiceBusProcessor Create(
        string? connectionName,
        string queueName,
        ServiceBusProcessorOptions? options = null)
        => new ServiceBusProcessorWrapper(
            clientProvider
                .GetClient(connectionName)
                .CreateProcessor(
                    queueName,
                    options));
}
