using IdentityServer4.AccessTokenValidation;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
var authenticationProviderKey = configuration["AuthServer:AuthenticationProviderKey"];
builder.Services.AddAuthentication("Bearer")
    .AddIdentityServerAuthentication(authenticationProviderKey, options =>
     {
         options.Authority = configuration["AuthServer:Authority"];
         options.ApiName = configuration["AuthServer:ApiName"];
         //options.ApiSecret = configuration["AuthServer:ApiSecret"];
         options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
         options.SupportedTokens = SupportedTokens.Both;
     });
builder.Services.AddOcelot();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IdentityApiGateway", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityApiGateway v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseOcelot().Wait();
app.Run();
