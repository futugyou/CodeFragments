using StackExchange.Redis.Profiling;

namespace AspnetcoreEx.RedisExtensions;

public class RedisProfiler
{
    private const string RequestContextKey = "RequestProfilingContext";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RedisProfiler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ProfilingSession? GetSession()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null) return null;
        return ctx.Items[RequestContextKey] as ProfilingSession;
    }

    public void CreateSessionForCurrentRequest()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
        {
            ctx.Items[RequestContextKey] = new ProfilingSession();
        }
    }
}