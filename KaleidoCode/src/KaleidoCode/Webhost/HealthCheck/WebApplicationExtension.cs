
namespace KaleidoCode.HealthCheck;

public static class WebApplicationExtension
{
    public static WebApplication UseHealthCheckExtensions(this WebApplication app)
    {
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        });

        app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
        return app;
    }

}