var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Webhost>("api");
    
builder.Build().Run();
