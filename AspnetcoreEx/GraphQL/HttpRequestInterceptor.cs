using HotChocolate.AspNetCore;
using HotChocolate.Execution;

namespace AspnetcoreEx.GraphQL;

public class HttpRequestInterceptor : DefaultHttpRequestInterceptor
{
    private readonly ILogger<HttpRequestInterceptor> logger;
    public HttpRequestInterceptor(ILogger<HttpRequestInterceptor> logger)
    {
        this.logger = logger;

    }
    public override ValueTask OnCreateAsync(HttpContext context, IRequestExecutor requestExecutor, IQueryRequestBuilder requestBuilder, CancellationToken cancellationToken)
    {
        if (context.Request.Headers.ContainsKey("X-Allow-Introspection"))
        {
            requestBuilder.AllowIntrospection();
        }
        else
        {
            // the header is not present i.e. introspection continues
            // to be disallowed
            requestBuilder.SetIntrospectionNotAllowedMessage(
                "Missing `X-Allow-Introspection` header");
        }
        var properties = new Dictionary<string, object>
        {
            { "testname", "testvalue" }
        };
        requestBuilder.SetProperties(properties!);
        logger.LogInformation("this log from HttpRequestInterceptor.OnCreateAsync");
        return base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}