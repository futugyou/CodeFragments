
namespace Microsoft.Extensions.Configuration.Json;

public class SimpleJsonConfigurationSource : JsonConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        // ensure it Must provider a FileProvider.
        EnsureDefaults(builder);
        return new SimpleJsonConfigurationProvider(this);
    }
}