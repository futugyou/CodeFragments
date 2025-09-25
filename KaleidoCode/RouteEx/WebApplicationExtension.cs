
namespace KaleidoCode.RouteEx;

public static class WebApplicationExtension
{
    public static WebApplication UseCustomRouteRewriter(this WebApplication app)
    {
        var rewriteOptions = new RewriteOptions()
            // client redirect
            .AddRedirect("^text/(.*)", "bar/$1")
            // server rewrite
            .AddRewrite(regex: "^text/(.*)", replacement: "bar/$1", skipRemainingRules: true)
            .AddIISUrlRewrite(fileProvider: app.Environment.ContentRootFileProvider, filePath: "rewrite.xml")
            .AddApacheModRewrite(fileProvider: app.Environment.ContentRootFileProvider, filePath: "rewrite.config");

        app.UseRewriter(rewriteOptions);

        return app;
    }

}