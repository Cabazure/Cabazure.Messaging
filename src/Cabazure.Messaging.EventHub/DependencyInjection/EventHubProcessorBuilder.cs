﻿using Azure.Messaging.EventHubs.Primitives;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

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

public record BlobContainerOptions(
    string ContainerName,
    bool CreateIfNotExist = false);
