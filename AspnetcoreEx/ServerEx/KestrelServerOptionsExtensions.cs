using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Reflection;
using System.Collections.Generic;

public static class KestrelServerOptionsExtensions
{
    public static IEnumerable<ListenOptions> GetListenOptions(this KestrelServerOptions options)
    {
        var property = typeof(KestrelServerOptions).GetProperty("ListenOptions", BindingFlags.NonPublic | BindingFlags.Instance);
        return (IEnumerable<ListenOptions>)property!.GetValue(options);
    }
}