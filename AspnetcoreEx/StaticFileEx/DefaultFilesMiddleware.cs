namespace AspnetcoreEx.StaticFileEx;

public class DefaultFilesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DefaultFilesOptions _options;

    public DefaultFilesMiddleware(RequestDelegate next, IWebHostEnvironment env, IOptions<DefaultFilesOptions> options)
    {
        _next = next;
        _options = options.Value;
        _options.FileProvider = _options.FileProvider ?? env.WebRootFileProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!new string[] { "GET", "HEAD" }.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var path = new PathString((context.Request.Path.Value ?? "").TrimEnd('/') + "/");
        PathString subPath;
        if (!path.StartsWithSegments(_options.RequestPath, out subPath))
        {
            await _next(context);
            return;
        }

        IDirectoryContents directoryContents = _options.FileProvider!.GetDirectoryContents(subPath);
        if (!directoryContents.Exists)
        {
            await _next(context);
            return;
        }

        foreach (var fileName in _options.DefaultFileNames)
        {
            if (_options.FileProvider.GetFileInfo($"{subPath}{fileName}").Exists)
            {
                if (_options.RedirectToAppendTrailingSlash && !(context.Request.Path.Value ?? "").EndsWith("/"))
                {
                    context.Response.StatusCode = 302;
                    context.Response.GetTypedHeaders().Location = new Uri(path.Value + context.Request.QueryString);
                    return;
                }

                context.Request.Path = new PathString($"{context.Request.Path}{fileName}");
            }
        }

        await _next(context);
    }
}