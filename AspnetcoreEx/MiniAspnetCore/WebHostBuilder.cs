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

    public static Task WriteAsync(this HttpResponse response ,string contents)
    {
        var buffer = System.Text.Encoding.UTF8.GetBytes(contents);
        return response.Body.WriteAsync(buffer, 0, buffer.Length);
    }

    public static RequestDelegate OneMiddleware (RequestDelegate next) => async context =>{
        await context.Response.WriteAsync("One=>");
        await next(context);
    };

    public static RequestDelegate TwoMiddleware (RequestDelegate next) => async context =>{
        await context.Response.WriteAsync("Two=>");
        await next(context);
    };

    public static RequestDelegate ThreeMiddleware (RequestDelegate next) => async context =>{
        await context.Response.WriteAsync("Three=>");
    };

    public static void StartMiniAspnetCore()
    {
        Host.CreateDefaultBuilder()
        .ConfigureWebHost(builder => builder
            .UseHttpListenerServer()
            .Configure(app => app
                .Use(OneMiddleware)
                .Use(TwoMiddleware)
                .Use(ThreeMiddleware)))
        .Build()
        .Run();
    }
}