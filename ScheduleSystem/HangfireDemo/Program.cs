using Hangfire;
using Hangfire.SqlServer;
using HangfireDemo.Service;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddScoped<IDosomething, Dosomething>();

// Add Hangfire services.
builder.Services.AddHangfire(configuration =>
{
    configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    })
    // auto delete job,defatul 1 day
    .WithJobExpirationTimeout(TimeSpan.FromDays(100));
});

// Add the processing server as IHostedService
builder.Services.AddHangfireServer(options =>
{
    // all default value
    options.WorkerCount = Math.Min(Environment.ProcessorCount * 5, 20);
    // that means priority:  "alpha" > "beta" > "default"
    // options.Queues = new[] { "alpha", "beta", "default" };
    options.Queues = new string[1] { "default" };
    options.StopTimeout = TimeSpan.FromMilliseconds(500.0);
    options.ShutdownTimeout = TimeSpan.FromSeconds(15.0);
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15.0);
    options.HeartbeatInterval = TimeSpan.FromSeconds(30.0);
    options.ServerTimeout = TimeSpan.FromMinutes(5.0);
    options.ServerCheckInterval = TimeSpan.FromMinutes(5.0);
    options.CancellationCheckInterval = TimeSpan.FromSeconds(5.0);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        //Authorization = new[] { new DemoAuthorizationFilter() }
    });
});

app.Run();
