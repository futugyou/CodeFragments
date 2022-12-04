using Microsoft.Extensions.Configuration.Json;

namespace AspnetcoreEx.Extensions;

public class CustomJsonConfigurationProvider : JsonConfigurationProvider
{
    public CustomJsonConfigurationProvider(CustomJsonConfigurationSource source) : base(source)
    {
    }
    public override void Load(Stream stream)
    {
        // Let the base class do the heavy lifting.
        base.Load(stream);

        // Do decryption here, you can tap into the Data property like so:

        Data["Client:Password"] = Data["Client:Password"] + "--thisistext";

        // But you have to make your own MyEncryptionLibrary, not included here
    }
}

public class CustomJsonConfigurationSource: JsonConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        // ensure it Must provider a FileProvider.
        EnsureDefaults(builder);
        return new CustomJsonConfigurationProvider(this);
    }
}