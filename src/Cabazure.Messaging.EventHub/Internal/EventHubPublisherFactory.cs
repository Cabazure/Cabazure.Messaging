﻿using System.Collections.Concurrent;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubPublisherFactory(
    IOptionsMonitor<CabazureEventHubOptions> options,
    IEnumerable<EventHubPublisherRegistration> registrations)
    : IEventHubPublisherFactory
    , IAsyncDisposable
{
    private sealed record ClientKey(string? Connection, string Topic);
    private sealed record PublisherKey(string? Connection, Type Type);
    private readonly ConcurrentDictionary<ClientKey, EventHubProducerClient> clientCache = [];
    private readonly Dictionary<PublisherKey, EventHubPublisherRegistration> publishers
        = registrations.ToDictionary(r => new PublisherKey(r.ConnectionName, r.Type));

    public IEventHubPublisher<TMessage> Create<TMessage>(
        string? connectionName = null)
    {
        var key = new PublisherKey(connectionName, typeof(TMessage));
        if (!publishers.TryGetValue(key, out var publisher))
        {
            throw new ArgumentException(
                $"Type {typeof(TMessage).Name} not configured as an EventHub publisher");
        }

        var client = clientCache.GetOrAdd(
            new(connectionName, publisher.EventHubName),
            CreateClient);

        var config = options.Get(connectionName);
        return new EventHubPublisher<TMessage>(
            config.SerializerOptions,
            client,
            publisher.PropertiesFactory,
            publisher.PartitionKeyFactory);
    }

    private EventHubProducerClient CreateClient(
        ClientKey key)
        => options.Get(key.Connection) switch
        {
            { FullyQualifiedNamespace: { } n, Credential: { } c } => new(n, key.Topic, c),
            { ConnectionString: { } cs } => new(cs, key.Topic),
            _ => throw new ArgumentException(
                $"Unknown connection name `{key.Connection}`")
        };


    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        foreach (var sender in clientCache.Values)
        {
            await sender.DisposeAsync();
        }
    }
}