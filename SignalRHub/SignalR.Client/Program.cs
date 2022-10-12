using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;
using SignalR.Client;
using SignalR.Common;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.AddSignalRServices(configuration);
services.AddSingleton<IClientService, ClientService>();
services.AddHostedService<ClientHostedService>();

var app = builder.Build();

app.MapGet("/", async ([FromServices] ISignalRPublisher publisher) =>
{
    var e = new CloudEvent
    {
        Id = Guid.NewGuid().ToString(),
        Data = "nothing",
    };

    await publisher.PublishAsync("thisistopic", "ssss");
});

app.Run();
