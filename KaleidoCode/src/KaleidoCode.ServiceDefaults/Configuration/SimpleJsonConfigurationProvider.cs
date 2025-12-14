
namespace Microsoft.Extensions.Configuration.Json;

public class SimpleJsonConfigurationProvider : JsonConfigurationProvider
{
    public SimpleJsonConfigurationProvider(SimpleJsonConfigurationSource source) : base(source)
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
