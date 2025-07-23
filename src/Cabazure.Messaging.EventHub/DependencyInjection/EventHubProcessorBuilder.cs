using Azure.Messaging.EventHubs.Primitives;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Event Hub processors with options such as filters, processor settings, and blob container configuration.
/// </summary>
public class EventHubProcessorBuilder(
    string eventHubName)
{
    public List<Func<IDictionary<string, object>, bool>> Filters { get; } = [];

    public EventProcessorOptions? ProcessorOptions { get; private set; }

    public BlobContainerOptions BlobContainer { get; private set; } = new(eventHubName, true);

    public EventHubProcessorBuilder WithFilter(
        Func<IDictionary<string, object>, bool> predicate)
    {
        Filters.Add(predicate);
        return this;
    }

    public EventHubProcessorBuilder WithProcessorOptions(
        EventProcessorOptions options)
    {
        ProcessorOptions = options;
        return this;
    }

    public EventHubProcessorBuilder WithBlobContainer(
        string containerName,
        bool createIfNotExist = true)
    {
        BlobContainer = new(containerName, createIfNotExist);
        return this;
    }
}

/// <summary>
/// Represents configuration options for the blob container used for Event Hub checkpointing.
/// </summary>
public record BlobContainerOptions(
    string ContainerName,
    bool CreateIfNotExist = false);
