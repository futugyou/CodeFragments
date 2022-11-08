using BenchmarkDotNet.Engines;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;

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
}
