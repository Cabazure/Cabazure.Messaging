using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Publishing;

public interface IServiceBusSenderProvider
{
    ServiceBusSender GetSender<T>(
        string? connectionName = null);
}