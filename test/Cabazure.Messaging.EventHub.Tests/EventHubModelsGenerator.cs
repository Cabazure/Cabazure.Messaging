using Atc.Test.Customizations;
using Atc.Test.Customizations.Generators;
using AutoFixture.Kernel;
using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.Tests;

[AutoRegister]
public class EventHubModelsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (request.IsRequestFor<EventData>())
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
}
