using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Webhost>("api");

var maf = builder.AddProject<Projects.AgentStack>("maf");

var graphql = builder.AddProject<Projects.GraphQLStack>("graphql");

builder.Build().Run();
