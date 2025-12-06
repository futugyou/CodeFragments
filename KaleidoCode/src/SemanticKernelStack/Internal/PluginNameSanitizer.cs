using System.Text.RegularExpressions;

namespace SemanticKernelStack.Internal;

internal static partial class PluginNameSanitizer
{
    [GeneratedRegex(@"[^A-Za-z0-9_]")]
    private static partial Regex UnsafeCharsRegex();

    public static string ToSafePluginName(string serverName)
    {
        return UnsafeCharsRegex().Replace(serverName, "_");
    }
}
