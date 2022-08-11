namespace Hangfire.Cap;

public static class HangfireCollectionExtensions
{
    public static IServiceCollection AddSelfHangfire(this IServiceCollection services, IConfiguration config)
    {
        var hangfireOption = new HangfireCustomOption();
        config.GetSection(HangfireCustomOption.ConfigSection).Bind(hangfireOption);
        var mqoption = new RabbitMQOptions();
        config.GetSection("RabbitMQ").Bind(mqoption);

        services.AddCap(x =>
        {
            x.UseSqlServer(hangfireOption.ConnectionString);
            x.UseRabbitMQ(r =>
            {
                r.UserName = mqoption.UserName;
                r.Password = mqoption.Password;
                r.Port = mqoption.Port;
            });
            x.UseDashboard();
            x.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        });

        services.AddTransient<ICapJob, CapJob>();

        var sqlStorageOptions = new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        };

        services.AddHangfire(configuration =>
        {
            configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(hangfireOption.ConnectionString, sqlStorageOptions)
            .UseConsole()
            .UseTagsWithSql(sqlOptions: sqlStorageOptions);
        });

        services.AddHangfireServer(options =>
        {
            options.ServerName = hangfireOption.ServiceName;
            options.WorkerCount = Math.Min(Environment.ProcessorCount * 5, hangfireOption.WorkerCount);
            options.Queues = hangfireOption.AllQueues;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(5);
            options.ServerTimeout = TimeSpan.FromMinutes(5.0);
            options.HeartbeatInterval = TimeSpan.FromSeconds(30.0);
            options.StopTimeout = TimeSpan.FromMilliseconds(500.0);
            options.ShutdownTimeout = TimeSpan.FromSeconds(15.0);
            options.ServerCheckInterval = TimeSpan.FromMinutes(5.0);
            options.CancellationCheckInterval = TimeSpan.FromSeconds(5.0);
        });

        return services;
    }

    public static IEndpointConventionBuilder AddSelfHangfireDashboard(this IEndpointRouteBuilder endpoints)
    {
        var dashbordConfig = new Hangfire.DashboardOptions
        {
            AppPath = "/hangfire",
            IgnoreAntiforgeryToken = true,
            DisplayStorageConnectionString = true,
        };
        var builder = endpoints.MapHangfireDashboard("/hangfire", dashbordConfig);

        return builder;
    }
}
