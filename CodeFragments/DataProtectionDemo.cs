using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

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

    static IDataProtectionProvider GetEphemeralDataProtectionProvider()
    {
        var services = new ServiceCollection();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        return services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>();
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

    public static void EphemeralDataProtectorUsecase()
    {
        var originalPayload = Guid.NewGuid().ToString();
        var dataProtectionProvider = GetEphemeralDataProtectionProvider();
        var protector = dataProtectionProvider.CreateProtector("foo");
        var protectedPayload = protector.Protect(originalPayload);

        protector = dataProtectionProvider.CreateProtector("foo");
        Debug.Assert(originalPayload == protector.Unprotect(protectedPayload));

        protector = GetEphemeralDataProtectionProvider().CreateProtector("foo");
        // CryptographicException: The payload was invalid.
        protector.Unprotect(protectedPayload);
    }

    public static void GeneratorHashUsecase()
    {
        var password ="password";
        var salt = new byte[16];
        var iteration = 1000;
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(salt);
        Console.WriteLine(Hash(KeyDerivationPrf.HMACSHA1));
        Console.WriteLine(Hash(KeyDerivationPrf.HMACSHA256));
        Console.WriteLine(Hash(KeyDerivationPrf.HMACSHA512));

        string Hash(KeyDerivationPrf prf)
        {
            var hashed = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: prf,
                iterationCount: iteration,
                numBytesRequested: 32
            );
            return Convert.ToBase64String(hashed);
        }
    }

    public static void FileSystemDataProtectorUsecase()
    {
        var dir = "./keyfiles"
        var services = new ServiceCollection();
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dir));
        var keyManager = services.BuildServiceProvider().GetRequiredService<IKeyManager>();
        var key1 = keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));
        var key2 = keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(2));
        var key3 = keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(3));

        Console.WriteLine(key1.KeyId);
        Console.WriteLine(key2.KeyId);
        Console.WriteLine(key3.KeyId);

        keyManager.RevokeKey(key1.KeyId);
        keyManager.RevokeAllKeys(DateTimeOffset.Now, "Revike All Keys");
    }
}