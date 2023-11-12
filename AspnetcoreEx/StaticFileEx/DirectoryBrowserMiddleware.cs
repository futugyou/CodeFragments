using System.Text.Encodings.Web;
using Microsoft.AspNetCore.StaticFiles;

namespace AspnetcoreEx.StaticFileEx;

public class DirectoryBrowserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DirectoryBrowserOptions _options;
    public DirectoryBrowserMiddleware(RequestDelegate next, IWebHostEnvironment env, IOptions<DirectoryBrowserOptions> options) : this(next, env, HtmlEncoder.Default, options)
    {

    }

    public DirectoryBrowserMiddleware(RequestDelegate next, IWebHostEnvironment env, HtmlEncoder encoder, IOptions<DirectoryBrowserOptions> options)
    {
        _next = next;
        _options = options.Value;
        _options.FileProvider ??= env.WebRootFileProvider;
        _options.Formatter ??= new HtmlDirectoryFormatter(encoder);
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

        if (_options.RedirectToAppendTrailingSlash && !(context.Request.Path.Value ?? "").EndsWith("/"))
        {
            context.Response.StatusCode = 302;
            context.Response.GetTypedHeaders().Location = new Uri(path.Value + context.Request.QueryString);
            return;
        }

        await _options.Formatter!.GenerateContentAsync(context, directoryContents);
    }
}