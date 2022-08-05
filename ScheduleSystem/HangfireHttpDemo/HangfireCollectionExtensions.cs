﻿using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.Heartbeat;
using Hangfire.Heartbeat.Server;
using Hangfire.HttpJob;
using Hangfire.SqlServer;
using Hangfire.Tags.SqlServer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Spring.Core.TypeConversion;
using System.Text;

namespace HangfireHttpDemo;
public static class HangfireCollectionExtensions
{
    private const string HangfireSettingsKey = "Hangfire:HangfireSettings";
    private const string HttpJobOptionsKey = "Hangfire:HttpJobOptions";
    private const string HangfireConnectStringKey = "Hangfire:HangfireSettings:ConnectionString";
    private const string HangfireLangKey = "Hangfire:HttpJobOptions:Lang";

    public static IServiceCollection AddSelfHangfire(this IServiceCollection services, IConfiguration config)
    {
        var sqlStorageOptions = new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        };
        // Add Hangfire services.
        services.AddHangfire(configuration =>
        {
            configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(config.GetConnectionString("HangfireConnection"), sqlStorageOptions)
            // auto delete job,defatul 1 day
            .WithJobExpirationTimeout(TimeSpan.FromDays(100))
            .UseConsole(new ConsoleOptions
            {
                BackgroundColor = "#CAFFFF"
            })
           .UseTagsWithSql(sqlOptions: sqlStorageOptions)
            .UseHangfireHttpJob(new()
            {

            })
            .UseHeartbeatPage();
        });

        // Add the processing server as IHostedService
        services.AddHangfireServer(options =>
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
        return services;
    }


    public static void ConfigurationHangfire(this IServiceCollection services, IConfiguration Configuration, IGlobalConfiguration globalConfiguration)
    {
        var serverProvider = services.BuildServiceProvider();
        var hangfireSettings = serverProvider.GetService<IOptions<HangfireSettings>>().Value;
        ConfigFromEnv(hangfireSettings);
        var httpJobOptions = serverProvider.GetService<IOptions<HangfireHttpJobOptions>>().Value;
        ConfigFromEnv(httpJobOptions);


        var sqlConnectStr = Configuration.GetSection(HangfireConnectStringKey).Get<string>();
        var envSqlConnectStr = GetEnvConfig<string>("HangfireRedisConnectionString");
        if (!string.IsNullOrEmpty(envSqlConnectStr)) sqlConnectStr = envSqlConnectStr;



    }

    public static IApplicationBuilder ConfigureSelfHangfire(this IApplicationBuilder app, IConfiguration Configuration)
    {
        var langStr = Configuration.GetSection(HangfireLangKey).Get<string>();
        var envLangStr = GetEnvConfig<string>("Lang");
        if (!string.IsNullOrEmpty(envLangStr)) langStr = envLangStr;
        if (!string.IsNullOrEmpty(langStr))
        {
            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(langStr)
            };
            app.UseRequestLocalization(options);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(langStr);
        }



        var services = app.ApplicationServices;
        var hangfireSettings = services.GetService<IOptions<HangfireSettings>>().Value;
        ConfigFromEnv(hangfireSettings);

        var queues = hangfireSettings.JobQueues.Select(m => m.ToLower()).Distinct().ToList();

        var workerCount = Math.Max(Environment.ProcessorCount, hangfireSettings.WorkerCount); //工作线程数，当前允许的最大线程，默认20

        //app.UseHangfireServer(new BackgroundJobServerOptions
        //{
        //    ServerName = hangfireSettings.ServerName,
        //    ServerTimeout = TimeSpan.FromMinutes(4),
        //    SchedulePollingInterval = TimeSpan.FromSeconds(1), //秒级任务需要配置短点，一般任务可以配置默认时间，默认15秒
        //    ShutdownTimeout = TimeSpan.FromMinutes(30), //超时时间
        //    Queues = queues.ToArray(), //队列
        //    WorkerCount = workerCount
        //}, additionalProcesses: new[] { new ProcessMonitor() });


        var dashbordConfig = new DashboardOptions
        {
            AppPath = "#",
            IgnoreAntiforgeryToken = true,
            DisplayStorageConnectionString = hangfireSettings.DisplayStorageConnectionString,
            IsReadOnlyFunc = Context => false
        };

        if (hangfireSettings.HttpAuthInfo.IsOpenLogin && hangfireSettings.HttpAuthInfo.Users.Any())
        {
            var httpAuthInfo = hangfireSettings.HttpAuthInfo;
            var users = hangfireSettings.HttpAuthInfo.Users.Select(m => new BasicAuthAuthorizationUser
            {
                Login = m.Login,
                Password = m.Password,
                PasswordClear = m.PasswordClear
            });

            var basicAuthAuthorizationFilterOptions = new BasicAuthAuthorizationFilterOptions
            {
                RequireSsl = httpAuthInfo.RequireSsl,
                SslRedirect = httpAuthInfo.SslRedirect,
                LoginCaseSensitive = httpAuthInfo.LoginCaseSensitive,
                Users = users
            };

            //dashbordConfig.Authorization = new[]
            //{
            //        new CustomAuthorizeFilter()
            //    };

        }

        app.UseHangfireDashboard(hangfireSettings.StartUpPath);

        if (!string.IsNullOrEmpty(hangfireSettings.ReadOnlyPath))
            //只读面板，只能读取不能操作 
            app.UseHangfireDashboard(hangfireSettings.ReadOnlyPath, new DashboardOptions
            {
                IgnoreAntiforgeryToken = true,
                AppPath = hangfireSettings.StartUpPath, //返回时跳转的地址
                DisplayStorageConnectionString = false, //是否显示数据库连接信息
                IsReadOnlyFunc = Context => true
            });

        return app;
    }
    #region Docker运行的参数配置https://github.com/yuzd/Hangfire.HttpJob/wiki/000.Docker-Quick-Start


    private static void ConfigFromEnv(HangfireSettings settings)
    {
        var HangfireQueues = GetEnvConfig<string>("HangfireQueues");
        if (!string.IsNullOrEmpty(HangfireQueues))
        {
            settings.JobQueues = HangfireQueues.Split(',').ToList();
        }

        var ServerName = GetEnvConfig<string>("ServerName");
        if (!string.IsNullOrEmpty(ServerName))
        {
            settings.ServerName = ServerName;
        }

        var TablePrefix = GetEnvConfig<string>("TablePrefix");
        if (!string.IsNullOrEmpty(TablePrefix))
        {
            settings.TablePrefix = TablePrefix;
        }

        var WorkerCount = GetEnvConfig<string>("WorkerCount");
        if (!string.IsNullOrEmpty(WorkerCount))
        {
            settings.WorkerCount = int.Parse(WorkerCount);
        }

        var HangfireUserName = GetEnvConfig<string>("HangfireUserName");
        var HangfirePwd = GetEnvConfig<string>("HangfirePwd");
        if (!string.IsNullOrEmpty(HangfireUserName) && !string.IsNullOrEmpty(HangfirePwd))
        {
            settings.HttpAuthInfo = new HttpAuthInfo { Users = new List<UserInfo>() };
            settings.HttpAuthInfo.Users.Add(new UserInfo
            {
                Login = HangfireUserName,
                PasswordClear = HangfirePwd
            });
        }
    }

    private static void ConfigFromEnv(HangfireHttpJobOptions settings)
    {
        var DefaultRecurringQueueName = GetEnvConfig<string>("DefaultRecurringQueueName");
        if (!string.IsNullOrEmpty(DefaultRecurringQueueName))
        {
            settings.DefaultRecurringQueueName = DefaultRecurringQueueName;
        }

        if (settings.MailOption == null) settings.MailOption = new MailOption();

        var HangfireMail_Server = GetEnvConfig<string>("HangfireMail_Server");
        if (!string.IsNullOrEmpty(HangfireMail_Server))
        {
            settings.MailOption.Server = HangfireMail_Server;
        }

        var HangfireMail_Port = GetEnvConfig<int>("HangfireMail_Port");
        if (HangfireMail_Port > 0)
        {
            settings.MailOption.Port = HangfireMail_Port;
        }

        var HangfireMail_UseSsl = Environment.GetEnvironmentVariable("HangfireMail_UseSsl");
        if (!string.IsNullOrEmpty(HangfireMail_UseSsl))
        {
            settings.MailOption.UseSsl = HangfireMail_UseSsl.ToLower().Equals("true");
        }

        var HangfireMail_User = GetEnvConfig<string>("HangfireMail_User");
        if (!string.IsNullOrEmpty(HangfireMail_User))
        {
            settings.MailOption.User = HangfireMail_User;
        }

        var HangfireMail_Password = GetEnvConfig<string>("HangfireMail_Password");
        if (!string.IsNullOrEmpty(HangfireMail_Password))
        {
            settings.MailOption.Password = HangfireMail_Password;
        }

    }
    private static T GetEnvConfig<T>(string key)
    {
        try
        {
            var value = Environment.GetEnvironmentVariable(key.Replace(":", "_"));
            if (!string.IsNullOrEmpty(value))
            {
                return (T)TypeConversionUtils.ConvertValueIfNecessary(typeof(T), value, null);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return default;
    }

    #endregion

}

public class HangfireSettings
{
    public string ServerName { get; set; }
    public string TablePrefix { get; set; }
    public string StartUpPath { get; set; }
    public string ReadOnlyPath { get; set; }
    public List<string> JobQueues { get; set; }
    public HttpAuthInfo HttpAuthInfo { get; set; } = new HttpAuthInfo();
    public int WorkerCount { get; set; } = 40;
    public bool DisplayStorageConnectionString { get; set; } = false;
}

public class HttpAuthInfo
{
    /// <summary>
    /// Redirects all non-SSL requests to SSL URL
    /// </summary>
    public bool SslRedirect { get; set; } = false;

    /// <summary>
    /// Requires SSL connection to access Hangfire dahsboard. It's strongly recommended to use SSL when you're using basic authentication.
    /// </summary>
    public bool RequireSsl { get; set; } = false;

    /// <summary>
    /// Whether or not login checking is case sensitive.
    /// </summary>
    public bool LoginCaseSensitive { get; set; } = true;

    public bool IsOpenLogin { get; set; } = true;

    public List<UserInfo> Users { get; set; } = new List<UserInfo>();
}

public class UserInfo
{
    public string Login { get; set; }
    public string PasswordClear { get; set; }

    public byte[] Password => Encoding.UTF8.GetBytes(PasswordClear);
}