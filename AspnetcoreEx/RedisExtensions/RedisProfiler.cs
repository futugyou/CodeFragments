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

    public ProfilingSession GetSession()
    {
        var ctx = _httpContextAccessor.HttpContext;
        var session = new ProfilingSession();
        if (ctx == null)
        {
            return session;
        }

        if (ctx.Items[RequestContextKey] is ProfilingSession storedSession)
        {
            return storedSession;
        }

        return session;
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