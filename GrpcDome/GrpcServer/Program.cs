using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Security.Claims;
using GrpcServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

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
app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<GreeterService>().RequireCors("AllowAll");
    endpoints.MapGrpcService<FourTypeService>();
    endpoints.MapGrpcService<ProtoTypeService>();
    endpoints.MapGet("/generateJwtToken", context =>
    {
        return context.Response.WriteAsync(GenerateJwtToken(context.Request.Query["name"]));
    });
    endpoints.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
});

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