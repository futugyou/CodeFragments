using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace CodeFragments;

public class DataProtectionDemo
{
    public static void BaseUsecase()
    {
        var originalPayload = Guid.NewGuid().ToString();
        var protectedPayload = Encrypt("foo", originalPayload);
        var unprotectedPayload = Dencrypt("foo", protectedPayload);
        Debug.Assert(originalPayload == unprotectedPayload);
    }

    static string Encrypt(string purpose, string originalPayload)
    {
        return GetDataProtector(purpose).Protect(originalPayload);
    }

    static string Dencrypt(string purpose, string protectedPayload)
    {
        return GetDataProtector(purpose).Unprotect(protectedPayload);
    }

    static IDataProtector GetDataProtector(string purpose)
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        return services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>().CreateProtector(purpose);
    }
}