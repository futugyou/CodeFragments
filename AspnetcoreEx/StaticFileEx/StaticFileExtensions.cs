using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using Microsoft.Extensions.FileProviders;

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
        };

        var defaultOptions = new DefaultFilesOptions
        {
            FileProvider = fileProvifer,
            RequestPath = "/documents",
        };
        defaultOptions.DefaultFileNames.Add("readme.md");

        app
            .UseDefaultFiles()
            .UseDefaultFiles(defaultOptions)
            .UseStaticFiles()
            .UseStaticFiles(fileOptions)
            .UseDirectoryBrowser().
            .UseDirectoryBrowser(directoryOptions);
        return app;
    }
}