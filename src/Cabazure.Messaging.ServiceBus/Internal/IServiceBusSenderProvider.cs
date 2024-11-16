using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public interface IServiceBusSenderProvider
{
    ServiceBusSender GetSender<T>(
        string? connectionName = null);
}