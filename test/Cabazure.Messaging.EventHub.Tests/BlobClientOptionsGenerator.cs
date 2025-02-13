using Atc.Test.Customizations;
using Atc.Test.Customizations.Generators;
using AutoFixture.Kernel;
using Azure.Storage.Blobs;

namespace Cabazure.Messaging.EventHub.Tests;

[AutoRegister]
public class BlobClientOptionsGenerator : ISpecimenBuilder
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (!request.IsRequestFor<BlobClientOptions>())
        {
            return new NoSpecimen();
        }

        return new BlobClientOptions();
    }
}
