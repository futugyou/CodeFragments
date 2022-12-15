using System.Collections.Specialized;
using System.Net;

namespace AspnetcoreEx.MiniAspnetCore;

public interface IFeatureCollection : IDictionary<Type, object?>
{

}

public class FeatureCollection : Dictionary<Type, object?>, IFeatureCollection
{

}

public static partial class MiniExtensions
{
    public static T? Get<T>(this IFeatureCollection feature) where T : class
    {
        return feature.TryGetValue(typeof(T), out var value) ? (T?)value : default;
    }

    public static IFeatureCollection Set<T>(this IFeatureCollection features, T? feature) where T : class
    {
        features[typeof(T)] = feature;
        return features;
    }
}

public interface IHttpRequestFeature
{
    Uri? Url { get; }
    NameValueCollection Headers { get; }
    Stream Body { get; }
}

public interface IHttpResponseFeature
{
    int StatusCode { get; set; }
    NameValueCollection Headers { get; }
    Stream Body { get; }
}

public class HttpListenerFeature : IHttpRequestFeature, IHttpResponseFeature
{
    private readonly HttpListenerContext _context;
    public HttpListenerFeature(HttpListenerContext context)
    {
        _context = context;
    }

    Uri? IHttpRequestFeature.Url => _context.Request.Url;

    NameValueCollection IHttpRequestFeature.Headers => _context.Request.Headers;

    Stream IHttpRequestFeature.Body => _context.Request.InputStream;

    int IHttpResponseFeature.StatusCode { get => _context.Response.StatusCode; set => _context.Response.StatusCode = value; }

    NameValueCollection IHttpResponseFeature.Headers => _context.Response.Headers;

    implStream IHttpResponseFeature.Body => _context.Response.OutputStream;
}