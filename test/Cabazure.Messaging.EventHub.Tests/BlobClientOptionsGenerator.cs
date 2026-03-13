using AutoFixture.Kernel;
using Azure.Storage.Blobs;
using Cabazure.Test.Customizations;

namespace Cabazure.Messaging.EventHub.Tests;

public class BlobClientOptionsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (SpecimenRequestHelper.GetRequestType(request) != typeof(BlobClientOptions))
        {
            return new NoSpecimen();
        }

        return new BlobClientOptions();
    }
}
