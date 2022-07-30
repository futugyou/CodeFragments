using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Plugin.Interrupt;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));
builder.Services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true; // default: false
    options.Scheduling.OverWriteExistingData = true; // default: true
});

//var properties = new NameValueCollection
//        {
//             { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"},
//             { "quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz"},
//             { "quartz.jobStore.tablePrefix", "QRTZ_"},
//             { "quartz.jobStore.dataSource", "Quartz.Sample"},
//             { "quartz.dataSource.Quartz.Sample.connectionString", @"Data Source=.;Initial Catalog=Quartz.Sample;Integrated Security=True"},
//             { "quartz.dataSource.Quartz.Sample.provider", "SqlServer"},
//             { "quartz.jobStore.clustered", "true"},
//             { "quartz.scheduler.instanceId", "AUTO"}
//        };

builder.Services.AddQuartz(q =>
{
    // handy when part of cluster or you want to otherwise identify multiple schedulers
    q.SchedulerId = "Scheduler-Core";

    // you can control whether job interruption happens for running jobs when scheduler is shutting down
    q.InterruptJobsOnShutdown = true;

    // when QuartzHostedServiceOptions.WaitForJobsToComplete = true or scheduler.Shutdown(waitForJobsToComplete: true)
    q.InterruptJobsOnShutdownWithWait = true;

    // we can change from the default of 1
    q.MaxBatchSize = 5;

    // we take this from appsettings.json, just show it's possible
    // q.SchedulerName = "Quartz ASP.NET Core Sample Scheduler";

    // this is default configuration if you don't alter it
    q.UseMicrosoftDependencyInjectionJobFactory();

    // these are the defaults
    q.UseSimpleTypeLoader();
    //q.UseInMemoryStore();
    q.UseDefaultThreadPool(maxConcurrency: 10);

    // quickest way to create a job with single trigger is to use ScheduleJob
    //q.ScheduleJob<ExampleJob>(trigger => trigger
    //    .WithIdentity("Combined Configuration Trigger")
    //    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(7)))
    //    .WithDailyTimeIntervalSchedule(x => x.WithInterval(10, IntervalUnit.Second))
    //    .WithDescription("my awesome trigger configured for a job with single call")
    //);

    // you can also configure individual jobs and triggers with code
    // this allows you to associated multiple triggers with same job
    // (if you want to have different job data map per trigger for example)
    //q.AddJob<ExampleJob>(j => j
    //    .StoreDurably() // we need to store durably if no trigger is associated
    //    .WithDescription("my awesome job")
    //);

    // here's a known job for triggers
    var jobKey = new JobKey("awesome job", "awesome group");
    //q.AddJob<ExampleJob>(jobKey, j => j
    //    .WithDescription("my awesome job")
    //);

    //q.AddTrigger(t => t
    //    .WithIdentity("Simple Trigger")
    //    .ForJob(jobKey)
    //    .StartNow()
    //    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(10)).RepeatForever())
    //    .WithDescription("my awesome simple trigger")
    //);

    //q.AddTrigger(t => t
    //    .WithIdentity("Cron Trigger")
    //    .ForJob(jobKey)
    //    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(3)))
    //    .WithCronSchedule("0/3 * * * * ?")
    //    .WithDescription("my awesome cron trigger")
    //);

    // auto-interrupt long-running job
    q.UseJobAutoInterrupt(options =>
    {
        // this is the default
        options.DefaultMaxRunTime = TimeSpan.FromMinutes(5);
    });
    //q.ScheduleJob<SlowJob>(
    //    triggerConfigurator => triggerConfigurator
    //        .WithIdentity("slowJobTrigger")
    //        .StartNow()
    //        .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()),
    //    jobConfigurator => jobConfigurator
    //        .WithIdentity("slowJob")
    //        .UsingJobData(JobInterruptMonitorPlugin.JobDataMapKeyAutoInterruptable, true)
    //        // allow only five seconds for this job, overriding default configuration
    //        .UsingJobData(JobInterruptMonitorPlugin.JobDataMapKeyMaxRunTime, TimeSpan.FromSeconds(5).TotalMilliseconds.ToString(CultureInfo.InvariantCulture)));

    const string calendarName = "myHolidayCalendar";
    q.AddCalendar<HolidayCalendar>(
        name: calendarName,
        replace: true,
        updateTriggers: true,
        x => x.AddExcludedDate(new DateTime(2020, 5, 15))
    );

    //q.AddTrigger(t => t
    //    .WithIdentity("Daily Trigger")
    //    .ForJob(jobKey)
    //    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
    //    .WithDailyTimeIntervalSchedule(x => x.WithInterval(10, IntervalUnit.Second))
    //    .WithDescription("my awesome daily time interval trigger")
    //    .ModifiedByCalendar(calendarName)
    //);

    // also add XML configuration and poll it for changes
    //q.UseXmlSchedulingConfiguration(x =>
    //{
    //    x.Files = new[] { "~/quartz_jobs.config" };
    //    x.ScanInterval = TimeSpan.FromMinutes(1);
    //    x.FailOnFileNotFound = true;
    //    x.FailOnSchedulingError = true;
    //});

    // convert time zones using converter that can handle Windows/Linux differences
    q.UseTimeZoneConverter();

    // add some listeners
    //q.AddSchedulerListener<SampleSchedulerListener>();
    //q.AddJobListener<SampleJobListener>(GroupMatcher<JobKey>.GroupEquals(jobKey.Group));
    //q.AddTriggerListener<SampleTriggerListener>();

    // example of persistent job store using JSON serializer as an example

    q.UsePersistentStore(s =>
    {
        s.UseProperties = true;
        s.RetryInterval = TimeSpan.FromSeconds(15);
        s.UseSqlServer(sqlServer =>
        {
            sqlServer.ConnectionString = configuration.GetConnectionString("QuartzConnection");
            // this is the default
            sqlServer.TablePrefix = "QRTZ_";
        });
        s.UseJsonSerializer();
        s.UseClustering(c =>
        {
            c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
            c.CheckinInterval = TimeSpan.FromSeconds(10);
        });
    });

});

builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
