using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace AspnetcoreEx.StaticFileEx;

public static class StaticFileExtensions
{
    public static WebApplication StaticFileComposite(this WebApplication app)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "doc");
        var fileProvifer = new PhysicalFileProvider(path);
        var contentTypeProvier = new FileExtensionContentTypeProvider();
        contentTypeProvier.Mappings.Add(".img", "image/jpg");

        var fileOptions = new StaticFileOptions
        {
            FileProvider = fileProvifer,
            RequestPath = "/documents",
            ContentTypeProvider = contentTypeProvier,
        };

        var directoryOptions = new DirectoryBrowerOptions
        {
            FileProvider = fileProvifer,
            RequestPath = "/documents",
            Formatter = new ListDirectoryFormatter(),
        };

        var defaultOptions = new DefaultFilesOptions
        {
            FileProvider = fileProvifer,
            RequestPath = "/documents",
        };
        defaultOptions.DefaultFileNames.Add("readme.md");

        app
            .UseMiddleware<DefaultFilesMiddleware>()
            .UseMiddleware<DefaultFilesMiddleware>(Options.Create(fileOptions))
            // .UseDefaultFiles()
            // .UseDefaultFiles(defaultOptions)
            
            .UseStaticFiles()
            .UseStaticFiles(fileOptions)
            
            .UseMiddleware<DirectoryBrowserMiddleware>()
            .UseMiddleware<DirectoryBrowserMiddleware>(Options.Create(directoryOptions))
            // .UseDirectoryBrowser()
            // .UseDirectoryBrowser(directoryOptions)
            ;
        return app;
    }
}