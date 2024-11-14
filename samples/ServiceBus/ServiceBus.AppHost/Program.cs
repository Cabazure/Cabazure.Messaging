var builder = DistributedApplication.CreateBuilder(args);

// Specified in the appsettings.Development.json
var serviceBus = builder.AddConnectionString("servicebus");

builder.AddProject<Projects.ServiceBus_Producer>("servicebus-producer")
    .WithReference(serviceBus);

builder.AddProject<Projects.ServiceBus_Processor>("servicebus-processor")
    .WithReference(serviceBus);

builder.Build().Run();
