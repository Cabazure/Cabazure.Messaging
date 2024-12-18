﻿using System.Text.Json;
using Azure.Core;

namespace Cabazure.Messaging.EventHub;

public class CabazureEventHubOptions
{
    public TokenCredential? Credential { get; set; }

    public string? FullyQualifiedNamespace { get; set; }

    public string? ConnectionString { get; set; }

    public BlobStorageOptions? BlobStorage { get; set; }

    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
    };

    public CabazureEventHubOptions WithSerializerOptions(JsonSerializerOptions options)
    {
        SerializerOptions = options;
        return this;
    }

    public CabazureEventHubOptions WithConnection(string fullyQualifiedNamespace, TokenCredential credential)
    {
        FullyQualifiedNamespace = fullyQualifiedNamespace;
        Credential = credential;
        return this;
    }

    public CabazureEventHubOptions WithConnection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }

    public CabazureEventHubOptions WithBlobStorage(
        Uri containerUri,
        TokenCredential credential,
        bool createIfNotExist = false)
    {
        BlobStorage = new()
        {
            ContainerUri = containerUri,
            Credential = credential,
            CreateIfNotExist = createIfNotExist,
        };
        return this;
    }

    public CabazureEventHubOptions WithBlobStorage(
        string connectionString,
        string blobContainerName,
        bool createIfNotExist = false)
    {
        BlobStorage = new()
        {
            ConnectionString = connectionString,
            ContainerName = blobContainerName,
            CreateIfNotExist = createIfNotExist,
        };
        return this;
    }

}

public class BlobStorageOptions
{
    public Uri? ContainerUri { get; set; }

    public TokenCredential? Credential { get; set; }

    public string? ConnectionString { get; set; }

    public string? ContainerName { get; set; }

    public bool CreateIfNotExist { get; set; }
}