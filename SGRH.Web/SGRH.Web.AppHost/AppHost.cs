var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.SGRH_Web_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.SGRH_Web_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.SGRH_Desktop>("sgrh-desktop");

builder.Build().Run();
