namespace Actors.Models;

public class Const
{
    public const string DaprSeparator = "||";
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(60);
    public static readonly TimeSpan DefaultOngoingCallTimeout = TimeSpan.FromSeconds(60);
    public const int DefaultReentrancyStackLimit = 32;
}
