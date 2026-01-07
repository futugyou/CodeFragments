using Microsoft.AspNetCore.StaticFiles;
using Path = System.IO.Path;

namespace KaleidoCode.StaticFileEx;

public static class StaticFileExtensions
{
    public static WebApplication StaticFileComposite(this WebApplication app)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "doc");
        var fileProvider = new PhysicalFileProvider(path);
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings.Add(".img", "image/jpg");

        var fileOptions = new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = "/documents",
            ContentTypeProvider = contentTypeProvider,
        };

        var directoryOptions = new DirectoryBrowserOptions
        {
            FileProvider = fileProvider,
            RequestPath = "/documents",
            Formatter = new ListDirectoryFormatter(),
        };

        var defaultOptions = new DefaultFilesOptions
        {
            FileProvider = fileProvider,
            RequestPath = "/documents",
        };
        defaultOptions.DefaultFileNames.Add("readme.md");

        app
            .UseMiddleware<DefaultFilesMiddleware>()
            .UseMiddleware<DefaultFilesMiddleware>(Options.Create(defaultOptions))
            .UseDefaultFiles()
            .UseDefaultFiles(defaultOptions)

            .UseStaticFiles()
            .UseStaticFiles(fileOptions)

            .UseMiddleware<DirectoryBrowserMiddleware>()
            .UseMiddleware<DirectoryBrowserMiddleware>(Options.Create(directoryOptions))
            .UseDirectoryBrowser()
            .UseDirectoryBrowser(directoryOptions)
            ;
        return app;
    }
}