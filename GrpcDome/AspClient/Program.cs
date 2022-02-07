using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Client.Web;
using GrpcClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
var defaultMethodConfig = new MethodConfig
{
    Names = { MethodName.Default },
    RetryPolicy = new RetryPolicy
    {
        MaxAttempts = 5,
        InitialBackoff = TimeSpan.FromSeconds(1),
        MaxBackoff = TimeSpan.FromSeconds(5),
        BackoffMultiplier = 1.5,
        RetryableStatusCodes = { StatusCode.Unavailable },
    }
};

builder.Services.AddGrpcClient<Greeter.GreeterClient>(o =>
{
    o.Address = new Uri("http://localhost:50001");
    o.ChannelOptionsActions.Add(channelOp =>
    {
        channelOp.ServiceConfig = new ServiceConfig
        {
            MethodConfigs = { defaultMethodConfig }
        };
        channelOp.MaxRetryAttempts = 5; // default  5
        channelOp.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB, default no limit
        channelOp.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB, default 4 MB
        channelOp.MaxRetryBufferSize = 16 * 1024 * 1024; // 16 MB, default 16 MB
        channelOp.MaxRetryBufferPerCallSize = 2 * 1024 * 1024; // 2 MB, default 1 MB
    });
})
.ConfigurePrimaryHttpMessageHandler(() => new GrpcWebHandler(new HttpClientHandler()))
.EnableCallContextPropagation(o => o.SuppressContextNotFoundErrors = true)
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    return handler;
})
;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
