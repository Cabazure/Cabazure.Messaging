var builder = DistributedApplication.CreateBuilder(args);

var blobs = builder
    .AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("blobs");
var eventHub = builder
    .AddAzureEventHubs("eh")
    .RunAsEmulator()
    .AddHub("eventhub");
eventHub.AddConsumerGroup("consumerGroup1");
eventHub.AddConsumerGroup("consumerGroup2");

for (int i = 1; i <= 4; i++)
{
    builder.AddProject<Projects.EventHub_Processor>(
        name: $"eventhub-processor{i}",
        launchProfileName: $"processor{i}")
        .WithEnvironment("CONSUMER_GROUP", "consumerGroup1")
        .WithReference(eventHub).WaitFor(eventHub)
        .WithReference(blobs).WaitFor(blobs);
}

for (int i = 1; i <= 4; i++)
{
    builder.AddProject<Projects.EventHub_Processor>(
        name: $"eventhub-processor-stateless{i}",
        launchProfileName: $"stateless{i}")
        .WithEnvironment("CONSUMER_GROUP", "consumerGroup2")
        .WithEnvironment("STATELESS", "true")
        .WithReference(eventHub).WaitFor(eventHub);
}

builder.AddProject<Projects.EventHub_Producer>("eventhub-producer")
    .WithReference(eventHub).WaitFor(eventHub);

builder.Build().Run();
