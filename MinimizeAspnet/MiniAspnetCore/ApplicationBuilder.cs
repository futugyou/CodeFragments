namespace MinimizeAspnet.MiniAspnetCore;

public interface IApplicationBuilder
{
    RequestDelegate Build();
    IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware);
}

public class ApplicationBuilder : IApplicationBuilder
{
    private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new List<Func<RequestDelegate, RequestDelegate>>();

    public RequestDelegate Build()
    {
        _middlewares.Reverse();
        RequestDelegate next = context =>
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        };
        foreach (var middleware in _middlewares)
        {
            next = middleware(next);
        }

        return next;
    }

    public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }
}