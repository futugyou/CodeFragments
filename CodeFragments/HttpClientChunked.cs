using BenchmarkDotNet.Engines;
using System.Net.Http.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CodeFragments;

// use https://webhook.site/
public class HttpClientChunked
{
    // Headers
    // connection  close
    // content-type application/json; charset=utf-8
    // transfer-encoding chunked
    // host webhook.site
    // content-length  24
    public static async Task PostWithChunked()
    {
        using var client = new HttpClient();
        await client.PostAsJsonAsync("https://webhook.site/a531b3a2-1ea3-46ec-add2-c37eaff84ab6", new { ClientIP = "127.0.0.1" });
    }

    // Headers
    // connection  close
    // content-length	24
    // content-type application/json; charset=utf-8
    // host webhook.site
    public static async Task PostWithoutChunked()
    {
        using var client = new HttpClient();

        var content = JsonContent.Create(new { ClientIP = "127.0.0.1" });
        await content.LoadIntoBufferAsync();
        var response = await client.PostAsync(
            "https://webhook.site/a531b3a2-1ea3-46ec-add2-c37eaff84ab6",
            content);
    }

    public static async Task HttpClientExceptionUsecase(string[] args)
    {
        var app = WebApplication.Create(args);
        app.MapGet("/", () => "Hello World!");
        await app.StartAsync();

        while (true)
        {
            using var httpclient = new HttpClient();
            try
            {
                var reply = await httpclient.GetStringAsync("http://localhost:5000");
                Debug.Assert(reply == "Hello World!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    // LifetimeTrackingHttpMessageHandler
    //   =>LoggingScopeHttpMessageHandler
    //      =>LoggingHttpMessageHandler
    //         =>HttpClientHandler
    public static async Task HttpClientHttpMessageHandlerUsecase()
    {
        var httpclient = new ServiceCollection()
            .AddHttpClient()
            .BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient();

        var handlerField = typeof(HttpMessageInvoker).GetField("_handler", BindingFlags.NonPublic | BindingFlags.Instance);
        PrintPipeline((HttpMessageHandler?)handlerField?.GetValue(httpclient), 0);
    }

    // LifetimeTrackingHttpMessageHandler
    //    =>LoggingScopeHttpMessageHandler
    //        =>OneHttpMessageHandler
    //            =>TwoHttpMessageHandler
    //                =>ThreeHttpMessageHandler
    //                    =>LoggingHttpMessageHandler
    //                        =>ExtendedHttpClientHandler
    public static async Task HttpClientCustomHttpMessageHandlerUsecase()
    {
        var services = new ServiceCollection();
        services.AddHttpClient(string.Empty)
            .ConfigurePrimaryHttpMessageHandler(_ => new ExtendedHttpClientHandler())
            .AddHttpMessageHandler(_ => new OneHttpMessageHandler())
            .AddHttpMessageHandler(_ => new TwoHttpMessageHandler())
            .AddHttpMessageHandler(_ => new ThreeHttpMessageHandler());

        var httpclient = services.BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient();
        
        var handlerField = typeof(HttpMessageInvoker).GetField("_handler", BindingFlags.NonPublic | BindingFlags.Instance);
        PrintPipeline((HttpMessageHandler?)handlerField?.GetValue(httpclient), 0);
    }

    static void PrintPipeline(HttpMessageHandler? handler, int index)
    {
        if (index == 0)
        {
            Console.WriteLine(handler?.GetType().Name);
        }
        else
        {
            Console.WriteLine($"{new string(' ', index * 4)}=>{handler?.GetType().Name}");
        }

        if (handler is DelegatingHandler delegatingHandler)
        {
            PrintPipeline(delegatingHandler.InnerHandler, index + 1);
        }
    }

    class ExtendedHttpClientHandler : HttpClientHandler{}
    class OneHttpMessageHandler : DelegatingHandler{}
    class TwoHttpMessageHandler : DelegatingHandler{}
    class ThreeHttpMessageHandler : DelegatingHandler{}
}
