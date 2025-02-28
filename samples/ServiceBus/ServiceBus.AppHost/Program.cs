var builder = DistributedApplication.CreateBuilder(args);

// Specified in the appsettings.Development.json
var serviceBus = builder
    .AddAzureServiceBus("servicebus")
    .RunAsEmulator();
var topic = serviceBus
    .AddServiceBusTopic("topic");
var subscription = topic
    .AddServiceBusSubscription("subscription");

builder.AddProject<Projects.ServiceBus_Producer>("servicebus-producer")
    .WithReference(topic);

builder.AddProject<Projects.ServiceBus_Processor>("servicebus-processor")
    .WithReference(subscription);

builder.Build().Run();
