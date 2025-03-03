using Atc.Test.Customizations;
using Atc.Test.Customizations.Generators;
using AutoFixture;
using AutoFixture.Kernel;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Cabazure.Messaging.StorageQueue.Tests;

[AutoRegister]
public class QueueModelsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (request.IsRequestFor<QueueClientOptions>())
        {
            return new QueueClientOptions();
        }
        
        if (request.IsRequestFor<QueueMessage>())
        {
            return QueuesModelFactory.QueueMessage(
                context.Create<string>(),
                context.Create<string>(),
                context.Create<string>(),
                context.Create<long>(),
                context.Create<DateTimeOffset>(),
                context.Create<DateTimeOffset>(),
                context.Create<DateTimeOffset>());
        }

        return new NoSpecimen();
    }
}
