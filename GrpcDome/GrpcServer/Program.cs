using System.IO.Compression;
using GrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);
// HTTP/2 over TLS is not supported on Windows versions earlier than Windows 10
builder.WebHost.ConfigureKestrel(op =>
{
    op.ListenLocalhost(50001, a =>
    {
        a.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB, default no limit
    options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB, default 4 MB
    options.EnableDetailedErrors = true; // default false
    options.ResponseCompressionLevel = CompressionLevel.SmallestSize; // default null 
});
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));

var app = builder.Build();
app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors();
// Configure the HTTP request pipeline.
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<GreeterService>().RequireCors("AllowAll");
    endpoints.MapGrpcService<FourTypeService>();
    endpoints.MapGrpcService<ProtoTypeService>();
});
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
