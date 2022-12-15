using System.Collections.Specialized;

namespace AspnetcoreEx.MiniAspnetCore;

public class HttpContext
{
    public HttpRequest Request{ get; }
    public HttpResponse Response{ get; }
}

public class HttpRequest
{
    public Uri Url { get; }
    public NameValueCollection Headers { get; }
    public Stream Body { get; }
}

public class HttpResponse
{
    public int StatusCode { get; set; }
    public NameValueCollection Headers { get; }
    public Stream Body { get; }
}