using System.Reflection;

namespace SemanticKernelStack.Resources;

internal static class EmbeddedResource
{
    private static readonly string? s_namespace = typeof(EmbeddedResource).Namespace;

    internal static string Read(string fileName)
    {
        Assembly? assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly;
        if (assembly == null) { throw new Exception($"[{s_namespace}] {fileName} assembly not found"); }

        // Resources are mapped like types, using the namespace and appending "." (dot) and the file name
        var resourceName = $"{s_namespace}." + fileName;
        using Stream? resource = assembly.GetManifestResourceStream(resourceName);
        if (resource == null) { throw new Exception($"{resourceName} resource not found"); }

        // Return the resource content, in text format.
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }
}
