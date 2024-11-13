using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Cabazure.Messaging.ServiceBus.Publishing;

public interface IServiceBusClientProvider
{
    ServiceBusClient GetClient(
        string? connectionName = null);

    ServiceBusAdministrationClient GetAdminClient(
        string? connectionName = null);
}