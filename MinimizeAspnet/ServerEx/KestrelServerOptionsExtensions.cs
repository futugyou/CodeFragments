using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Reflection;

namespace KaleidoCode.MinimizeAspnet;

public static class KestrelServerOptionsExtensions
{
    public static IEnumerable<ListenOptions> GetListenOptions(this KestrelServerOptions options)
    {
        IEnumerable<ListenOptions> list = new List<ListenOptions>();
        var property = typeof(KestrelServerOptions).GetProperty("ListenOptions", BindingFlags.NonPublic | BindingFlags.Instance);
        if (property == null)
        {
            return list;
        }

        if (property.GetValue(options) is IEnumerable<ListenOptions> oplist)
        {
            return oplist;
        }

        return list;
    }
}