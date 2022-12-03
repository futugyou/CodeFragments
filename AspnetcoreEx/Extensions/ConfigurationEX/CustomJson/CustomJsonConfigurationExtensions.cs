namespace AspnetcoreEx.Extensions;

public static class CustomJsonConfigurationExtensions
{
    public static IConfigurationBuilder AddJsonFileExtensions(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("File path must be a non-empty string.");
        }

        var source = new CustomJsonConfigurationSource
        {
            FileProvider = null,
            Path = path,
            Optional = optional,
            ReloadOnChange = reloadOnChange
        };

        source.ResolveFileProvider();
        builder.Add(source);
        return builder;
    }
}
