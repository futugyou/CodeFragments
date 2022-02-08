using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;
using GrpcServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProtoBuf.Grpc.Server;

SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());
JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
var builder = WebApplication.CreateBuilder(args);
// HTTP/2 over TLS is not supported on Windows versions earlier than Windows 10
builder.WebHost.ConfigureKestrel(op =>
{
    op.ListenLocalhost(50001, a =>
    {
        a.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
    op.ListenLocalhost(50002, a =>
    {
        a.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddCodeFirstGrpc();
builder.Services.AddGrpc(options =>
{
    options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB, default no limit
    options.MaxReceiveMessageSize = 2 * 1024 * 1024; // 2 MB, default 4 MB
    options.EnableDetailedErrors = true; // default false
    options.ResponseCompressionLevel = CompressionLevel.SmallestSize; // default null 
    options.Interceptors.Add<ServerLoggerInterceptor>();
});
builder.Services.AddGrpcHttpApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
builder.Services.AddGrpcSwagger();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
}));
builder.Services
.AddGrpcHealthChecks()
.AddAsyncCheck("", () =>
{
    var result = HealthCheckResult.Healthy();
    return Task.FromResult(result);
}, new string[] { "ok" });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireClaim(ClaimTypes.Name);
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateActor = false,
            ValidateLifetime = true,
            IssuerSigningKey = SecurityKey
        };
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});
app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcHealthChecksService();
    endpoints.MapGrpcService<GreeterService>().RequireCors("AllowAll");
    endpoints.MapGrpcService<FourTypeService>();
    endpoints.MapGrpcService<ProtoTypeService>();
    endpoints.MapGrpcService<OrderService>();
    endpoints.MapGet("/generateJwtToken", context =>
    {
        return context.Response.WriteAsync(GenerateJwtToken(context.Request.Query["name"]));
    });
    endpoints.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
});
app.MapGrpcHealthChecksService();
app.Run();


string GenerateJwtToken(string name)
{
    if (string.IsNullOrEmpty(name))
    {
        throw new InvalidOperationException("Name is not specified.");
    }

    var claims = new[] { new Claim(ClaimTypes.Name, name) };
    var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken("ExampleServer", "ExampleClients", claims, expires: DateTime.Now.AddSeconds(60), signingCredentials: credentials);
    return JwtTokenHandler.WriteToken(token);
}