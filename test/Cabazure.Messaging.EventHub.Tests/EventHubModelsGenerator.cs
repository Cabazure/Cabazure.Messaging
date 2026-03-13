using AutoFixture.Kernel;
using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.Tests;

public class EventHubModelsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (IsRequestFor<EventData>(request))
        {
            return EventHubsModelFactory.EventData(
                eventBody: context.Create<BinaryData>(),
                properties: context.Create<Dictionary<string, object>>(),
                systemProperties: context.Create<Dictionary<string, object>>(),
                partitionKey: context.Create<string>(),
                sequenceNumber: context.Create<long>(),
                offsetString: context.Create<string>(),
                enqueuedTime: context.Create<DateTimeOffset>());
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
