using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Webhost>("api");

var maf = builder.AddProject<Projects.AgentStack>("maf");

builder.Build().Run();
