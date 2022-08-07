using Hangfire.HttpJob.Agent.MssqlConsole;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHangfireJobAgent();

var app = builder.Build();

app.UseHangfireJobAgent(option =>
{
    option
    .Enabled(true)
    .EnabledBasicAuth(false)
    .WithEnableAutoRegister(true)
    .WithSitemap("/jobagent")
    .WithRegisterAgentHost("https://localhost:7139")
    .WithRegisterHangfireUrl("https://localhost:7100/hangfire")
    ;
});

app.Run();
