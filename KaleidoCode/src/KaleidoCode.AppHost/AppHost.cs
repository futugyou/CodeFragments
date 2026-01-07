using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.KaleidoCode>("api");

var maf = builder.AddProject<Projects.AgentStack>("maf");

var graphql = builder.AddProject<Projects.GraphQLStack>("graphql");

var es = builder.AddProject<Projects.OpenSearchStack>("es");

var sk = builder.AddProject<Projects.SemanticKernelStack>("sk");

var km = builder.AddProject<Projects.KernelMemoryStack>("km");

builder.Build().Run();
