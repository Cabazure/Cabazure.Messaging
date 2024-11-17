using FluentAssertions.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Messaging.ServiceBus.Tests;

public static class FluentAssertionsExtensions
{
    public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> Contain<TService, TImplementation>(
        this GenericCollectionAssertions<ServiceDescriptor> collection)
        => collection
            .Contain(descriptor
                => descriptor.ServiceType == typeof(TService)
                && descriptor.ImplementationType == typeof(TImplementation));

    public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> Contain<TService, TImplementation>(
        this GenericCollectionAssertions<ServiceDescriptor> collection,
        ServiceLifetime lifetime)
        => collection
            .Contain(descriptor
                => descriptor.ServiceType == typeof(TService)
                && descriptor.ImplementationType == typeof(TImplementation)
                && descriptor.Lifetime == lifetime);

    public static AndWhichConstraint<GenericCollectionAssertions<ServiceDescriptor>, ServiceDescriptor> Contain<TService>(
        this GenericCollectionAssertions<ServiceDescriptor> collection,
         ServiceLifetime lifetime)
        => collection
            .Contain(descriptor
                => descriptor.ServiceType == typeof(TService)
                && descriptor.Lifetime == lifetime);
}
