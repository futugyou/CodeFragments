
using System.Security.Cryptography;

namespace JwtBearerService;

public class Util
{
    public static DateTime UnixTimeStampToDateTime(long expiryDateUnix)
    {
        return TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local).AddSeconds(expiryDateUnix);
    }

    public static long DateTimeToUnixTime(DateTime dateTime)
    {
        return (long)(dateTime - TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local)).TotalSeconds;
    }

    public static string GenerateRandomNumber(int len = 32)
    {
        var randomNumber = new byte[len];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}