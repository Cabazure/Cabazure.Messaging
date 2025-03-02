using Atc.Test.Customizations;
using Atc.Test.Customizations.Generators;
using AutoFixture.Kernel;
using Azure.Storage.Queues;

namespace Cabazure.Messaging.StorageQueue.Tests;

[AutoRegister]
public class QueueClientOptionsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (!request.IsRequestFor<QueueClientOptions>())
        {
            return new NoSpecimen();
        }

        return new QueueClientOptions();
    }
}
