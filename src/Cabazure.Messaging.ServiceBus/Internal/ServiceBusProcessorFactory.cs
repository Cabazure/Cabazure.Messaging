using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public interface IServiceBusProcessorFactory
{
    ServiceBusProcessor Create(
        string? connectionName,
        string queueName,
        ServiceBusProcessorOptions? options = null);

    ServiceBusProcessor Create(
        string? connectionName,
        string topicName,
        string subscriptionName,
        ServiceBusProcessorOptions? options = null);
}

public class ServiceBusProcessorFactory(
    IServiceBusClientProvider clientProvider)
    : IServiceBusProcessorFactory
{
    public ServiceBusProcessor Create(
        string? connectionName,
        string topicName,
        string subscriptionName,
        ServiceBusProcessorOptions? options = null)
        => clientProvider
            .GetClient(connectionName)
            .CreateProcessor(
                topicName,
                subscriptionName,
                options);

    public ServiceBusProcessor Create(
        string? connectionName,
        string queueName,
        ServiceBusProcessorOptions? options = null)
        => clientProvider
            .GetClient(connectionName)
            .CreateProcessor(
                queueName,
                options);
}
