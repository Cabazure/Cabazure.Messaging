﻿using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

public class EventHubProcessorBuilder(
    string eventHubName)
{
    public List<Func<IDictionary<string, object>, bool>> Filters { get; } = [];

    public TimeSpan CheckpointMaxAge { get; set; } = TimeSpan.FromHours(1);

    public int CheckpointMaxEvents { get; set; } = 25;

    public EventProcessorClientOptions? ProcessorOptions { get; private set; }

    public BlobContainerOptions BlobContainer { get; private set; } = new(eventHubName, true);

    public EventHubProcessorBuilder WithFilter(
        Func<IDictionary<string, object>, bool> predicate)
    {
        Filters.Add(predicate);
        return this;
    }

    public EventHubProcessorBuilder WithProcessorOptions(
        EventProcessorClientOptions options)
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

    public EventHubProcessorBuilder WithCheckpointMaxAge(
        TimeSpan age)
    {
        CheckpointMaxAge = age;
        return this;
    }

    public EventHubProcessorBuilder WithCheckpointMaxEvents(
        int eventCount)
    {
        CheckpointMaxEvents = eventCount;
        return this;
    }
}

public record BlobContainerOptions(
    string ContainerName,
    bool CreateIfNotExist = false);
