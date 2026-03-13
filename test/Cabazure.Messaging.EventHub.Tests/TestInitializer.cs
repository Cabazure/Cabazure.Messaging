using Azure.Messaging.EventHubs;
using System.Runtime.CompilerServices;

namespace Cabazure.Messaging.EventHub.Tests;

internal static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        FixtureFactory.Customizations.Add(new BlobClientOptionsGenerator());
        FixtureFactory.Customizations.Add(new EventHubModelsGenerator());
        FixtureFactory.Customizations.Add(_ => new EventHubsRetryOptions());
    }
}
