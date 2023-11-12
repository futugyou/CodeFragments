namespace AspnetcoreEx.MiniWebApplication;


internal static class HostingPathResolver
{
    public static string ResolvePath(string? contentRootPath) =>
        PathWithDirectorySeperatorAtEnd(ResolvePathNonCononical(contentRootPath, AppContext.BaseDirectory));
 
    public static string ResolvePath(string? contentRootPath, string basePath) =>
        PathWithDirectorySeperatorAtEnd(ResolvePathNonCononical(contentRootPath, basePath));
 
    private static string PathWithDirectorySeperatorAtEnd(string path) =>
        System.IO.Path.EndsInDirectorySeparator(path) ? path : path + System.IO.Path.DirectorySeparatorChar;
 
    private static string ResolvePathNonCononical(string? contentRootPath, string basePath)
    {
        if (string.IsNullOrEmpty(contentRootPath))
        {
            return System.IO.Path.GetFullPath(basePath);
        }

        if (System.IO.Path.IsPathRooted(contentRootPath))
        {
            return System.IO.Path.GetFullPath(contentRootPath);
        }

        return System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetFullPath(basePath), contentRootPath));
    }
}