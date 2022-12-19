using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace AspnetcoreEx.ExceptionEx;

public class ExceptionExtensions
{
    public static RequestDelegate BuilderHandler(IEndpointRouteBuilder endpoints, bool allowStatusCode404Response)
    {
        var options = new ExceptionHandlerOptions
        {
            ExceptionHandler = httpContext =>
            {
                httpContext.Response.StatusCode =404;
                return Task.CompletedTask;
            },
            AllowStatusCode404Response = allowStatusCode404Response
        };

        var app = endpoints.CreateApplicationBuilder();
        app
            .UseExceptionHandler(options)
            .Run(httpContext => Task.FromExcption(new InvalidOperationException("Manually thrown exception.")));
        return app.Build();
    }
}