using AgentStack;
using AgentStack.Api;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAgentServices(configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var httpContext = context.ApplicationServices.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        HttpRequest request = httpContext.Request!;
        string scheme = request.Headers["X-Forwarded-Proto"].FirstOrDefault()
                        ?? (request.IsHttps ? "https" : "https");//The purpose is to emphasize that it must be HTTPS.

        string host = request.Host.Value!;

        document.Servers = [new OpenApiServer { Url = $"{scheme}://{host}" }];

        // The "/" approach is a more versatile way of doing things; 
        // it doesn't require configuring UseForwardedHeaders, 
        // nor does it require configuring IHttpContextAccessor, and it doesn't require checking IsHttps.
        // However, we are intentionally not doing it this way here.
        // document.Servers = [new OpenApiServer { Url = "/" }];

        return Task.CompletedTask;
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// This middleware will cause problems when the frontend proxy uses HTTPS and the backend runs on HTTP.
// app.UseHttpsRedirection();
// No need for now
// app.UseStaticFiles();
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseAgentEndpoints();
app.UseWorkflowEndpoints();
app.MapAguiExtensions();

app.Run();
