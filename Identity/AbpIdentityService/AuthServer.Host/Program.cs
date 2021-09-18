using AuthServer.Host.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;

namespace AuthServer.Host
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Log.Information("Starting AuthServer.Host.");
                var host = CreateHostBuilder(args).Build();
                host.MigrateDbContext<AuthServerDbContext>((_, __) => { });
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "AuthServer.Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseAutofac()
                .UseSerilog(InitSerilog);


        private static readonly Action<HostBuilderContext, LoggerConfiguration> InitSerilog = (context, config) =>
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            string logOutputTemplate = "{Timestamp:HH:mm:ss.fff zzz} || {Level} || {SourceContext:l} || {Message} || {Exception} ||end {NewLine}";

            config
            .Enrich.FromLogContext()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.WithProperty("app", context.HostingEnvironment.ApplicationName)
            .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
            .WriteTo.File($"{AppContext.BaseDirectory}Logs/serilog.log", rollingInterval: RollingInterval.Day, outputTemplate: logOutputTemplate);

            if (!string.IsNullOrEmpty(configuration["GrafanaLoki:Uri"]))
            {
                config.WriteTo.GrafanaLoki(configuration["GrafanaLoki:Uri"]);
            }
            if (!string.IsNullOrEmpty(configuration["ElasticSearch:Uri"]))
            {
                config.WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(configuration["ElasticSearch:Url"]))
                    {
                        AutoRegisterTemplate = true,
                        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                        IndexFormat = "auth-log-{0:yyyy.MM}"
                    });
            }
        };
    }
}
