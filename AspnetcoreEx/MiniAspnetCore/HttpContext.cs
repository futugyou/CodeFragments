using System.Collections.Specialized;

namespace AspnetcoreEx.MiniAspnetCore;

public class HttpContext
{
    public HttpContext(IFeatureCollection features)
    {

    }
    public HttpRequest Request { get; }
    public HttpResponse Response { get; }
}

public class HttpRequest
{
    private readonly IHttpRequestFeature _feature;
    public HttpRequest(IFeatureCollection features)
    {
        _feature = features.Get<IHttpRequestFeature>() ?? throw new InvalidOperationException("IHttpRquestFeature does not exist.");
    }

    public Uri? Url => _feature.Url;
    public NameValueCollection Headers => _feature.Headers;
    public Stream Body => _feature.Body;
}

public class HttpResponse
{
    private readonly IHttpResponseFeature _feature;
    public HttpResponse(IFeatureCollection features)
    {
        _feature = features.Get<IHttpResponseFeature>() ?? throw new InvalidOperationException("IHttpResponseFeature does not exist.");
    }

    public int StatusCode
    {
        get => _feature.StatusCode;
        set => _feature.StatusCode = value;
    }

    public NameValueCollection Headers => _feature.Headers;
    public Stream Body => _feature.Body;
}