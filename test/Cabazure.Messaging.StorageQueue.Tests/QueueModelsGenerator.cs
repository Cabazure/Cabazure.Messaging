using AutoFixture.Kernel;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Cabazure.Messaging.StorageQueue.Tests;

public class QueueModelsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (IsRequestFor<QueueClientOptions>(request))
        {
            return new QueueClientOptions();
        }

        if (IsRequestFor<QueueMessage>(request))
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

    private static bool IsRequestFor<T>(object request)
        => request switch
        {
            Type type => type == typeof(T),
            SeededRequest seededRequest => Equals(seededRequest.Request, typeof(T)),
            _ => false,
        };
}
