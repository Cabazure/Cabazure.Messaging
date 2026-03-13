using AutoFixture.Kernel;
using Azure.Storage.Blobs;

namespace Cabazure.Messaging.EventHub.Tests;

public class BlobClientOptionsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (!IsRequestFor<BlobClientOptions>(request))
        {
            return new NoSpecimen();
        }

        return new BlobClientOptions();
    }

    private static bool IsRequestFor<T>(object request)
        => request switch
        {
            Type type => type == typeof(T),
            SeededRequest seededRequest => Equals(seededRequest.Request, typeof(T)),
            _ => false,
        };
}
