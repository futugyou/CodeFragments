namespace AspnetcoreEx.MiniAspnetCore;

public interface IServer
{
    Task StartAsync(RequestDelegate handler);
}

public class HttpListenerServer : IServer
{
    private readonly HttpListener _httpListener;
    private readonly string[] _urls;
    public HttpListenerServer(params string[] urls)
    {
        _httpListener = new HttpListener();
        _urls = urls.Any() ? urls : new string[]{ "http://localhost:5005/" };
    }

    public Task StartAsync(RequestDelegate handler)
    {
        Array.ForEach(_urls, url => _httpListener.Prefixes.Add(url));
        _httpListener.Start();
        while (true)
        {
            _ = ProcessAsync(handler);
        }

        async Task ProcessAsync(RequestDelegate handler)
        {
            try
            {
                var listnerContext = await _httpListener.GetContextAsync();
                var feature = new HttpListenerFeature(listnerContext);
                var features = new FeatureCollection()
                    .Set<IHttpRequestFeature>(feature)
                    .Set<IHttpResponseFeature>(feature);
                var httpContext = new HttpContext(features);
                await handler(httpContext);
                listnerContext.Response.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }           
        }
    }
}