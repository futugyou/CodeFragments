using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
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

    static string Encrypt(string purpose, string originalPayload, TimeSpan timeout)
    {
        return (GetDataProtector(purpose) as ITimeLimitedDataProtector).Protect(originalPayload, DateTimeOffset.UtcNow.Add(timeout));
    }

    static string Dencrypt(string purpose, string protectedPayload)
    {
        return GetDataProtector(purpose).Unprotect(protectedPayload);
    }

    static IDataProtector GetDataProtector(string purpose)
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        //return services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>().CreateProtector(purpose);
        // return services.BuildServiceProvider().GetDataProtectionProvider().CreateProtector(purpose);
        return services.BuildServiceProvider().GetDataProtector(purpose).ToTimeLimitedDataProtector();
    }

    public static async Task DataProtectionWithTimeLimitUsecase()
    {
        var originalPayload = Guid.NewGuid().ToString();
        var protectedPayload = Encrypt("foo", originalPayload, TimeSpan.FromSeconds(5));
        var unprotectedPayload = Dencrypt("foo", protectedPayload);
        Debug.Assert(originalPayload == unprotectedPayload);

        await Task.Delay(5000);
        // it will throw CryptographicException: The payload expired at ****
        Dencrypt("foo", protectedPayload);
    }

    public static void DataProtectorRevokeUsecase()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        var serviceProvider = services.BuildServiceProvider();
        var protector = serviceProvider.GetDataProtector("foo");
        var originalPayload = Guid.NewGuid().ToString();
        var protectedPayload = protector.Protect(originalPayload);
        
        var keyRingProvider = serviceProvider.GetRequiredService<IKeyRingProvider>();
        var keyRing = keyRingProvider.GetCurrentKeyRing();
        var keyManager = serviceProvider.GetRequiredService<IKeyManager>();
        
        // keyManager.RevokeKey(keyRing.DefaultKeyId);
        keyManager.RevokeAllKeys(revocationDate: DateTimeOffset.UtcNow, reason: "no reason");
        // CryptographicException: The key {1d12aa41-xxxx-xxxx-xxxx-xxxxxxxxx} has been revoked
        protector.Unprotect(protectedPayload);
    }
}