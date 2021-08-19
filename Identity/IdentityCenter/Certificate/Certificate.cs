using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace IdentityCenter.Certificate;
public static class Certificate
{
    public static X509Certificate2 Get()
    {
        var assembly = typeof(Certificate).GetTypeInfo().Assembly;
        // this is only for dev env.
        using var stream = assembly.GetManifestResourceStream("IdentityCenter.Certificate.IdentityCenter.pfx");
        if (stream == null) throw new Exception("no certificate!");
        return new X509Certificate2(ReadStream(stream), "identitycenter");
    }

    private static byte[] ReadStream(Stream input)
    {
        byte[] buffer = new byte[16 * 1024];
        using var ms = new MemoryStream();
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }
}
