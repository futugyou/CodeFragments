using Microsoft.AspNetCore.StaticFiles;

namespace KaleidoCode.StaticFileEx;

public class ListDirectoryFormatter : IDirectoryFormatter
{
    public async Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
    {
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync("<html><head><title>Index</title></head<body><ul>");
        foreach (var file in contents)
        {
            string href = $"{context.Request.Path.Value?.TrimEnd('/')}/{file.Name}";
            await context.Response.WriteAsync($"<li><a href='{href}'>{file.Name}</a></li>");
        }

        await context.Response.WriteAsync("</ul></body></html>");
    }
}