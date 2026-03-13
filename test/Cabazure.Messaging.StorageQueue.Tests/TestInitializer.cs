using System.Runtime.CompilerServices;

namespace Cabazure.Messaging.StorageQueue.Tests;

internal static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
        => FixtureFactory.Customizations.Add(new QueueModelsGenerator());
}
