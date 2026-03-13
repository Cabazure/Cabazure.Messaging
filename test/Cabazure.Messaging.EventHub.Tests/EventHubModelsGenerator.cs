using AutoFixture;
using Azure.Messaging.EventHubs;
using Cabazure.Test.Customizations;

namespace Cabazure.Messaging.EventHub.Tests;

public class EventHubModelsGenerator : TypeCustomization<EventData>
{
    public EventHubModelsGenerator()
        : base(fixture => EventHubsModelFactory.EventData(
            eventBody: fixture.Create<BinaryData>(),
            properties: fixture.Create<Dictionary<string, object>>(),
            systemProperties: fixture.Create<Dictionary<string, object>>(),
            partitionKey: fixture.Create<string>(),
            sequenceNumber: fixture.Create<long>(),
            offsetString: fixture.Create<string>(),
            enqueuedTime: fixture.Create<DateTimeOffset>()))
    {
    }
}
