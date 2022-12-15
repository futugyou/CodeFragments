namespace AspnetcoreEx.MiniAspnetCore;

public class WebHostBuilder
{
    public IHostBuilder HostBuilder { get; }
    public IApplicationBuilder ApplicationBuilder { get; }
    public WebHostBuilder(IHostBuilder hostBuilder, IApplicationBuilder applicationBuilder)
    {
        HostBuilder = hostBuilder;
        ApplicationBuilder = applicationBuilder;
    }
}

public static partial class MiniExtensions
{
    public static WebHostBuilder UseHttpListenerServer(this WebHostBuilder builder, params string[] urls)
    {
        builder.HostBuilder.ConfigureServices(svcs => svcs.AddSingleton<IServer>(new HttpListenerServer(urls)));
        return builder;
    }

    public static WebHostBuilder Configure(this WebHostBuilder builder, Action<IApplicationBuilder> configure)
    {
        configure?.Invoke(builder.ApplicationBuilder);
        return builder;
    } 

    public static IHostBuilder ConfigureWebHost(this IHostBuilder builder, Action<WebHostBuilder> configure)
    {
        var webHostBuilder = new WebHostBuilder(builder, new ApplicationBuilder());
        configure?.Invoke(webHostBuilder);
        builder.ConfigureServices(svcs => svcs.AddSingleton<IHostedService>(provider => 
        {
            var server = provider.GetRequiredService<IServer>();
            var handler = webHostBuilder.ApplicationBuilder.Build();
            return new WebHostedService(server, handler);
        }));
        return builder;
    }
}