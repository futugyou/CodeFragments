
namespace Microsoft.Extensions.Configuration;

public static class JsonNodeExtensions
{
    /// <summary>
    /// Recursively enumerates all JsonNodes in the given JsonNode object in document order, including itself.
    /// </summary>
    public static IEnumerable<JsonNode?> DescendantsAndSelf(this JsonNode? root)
    {
        if (root == null)
        {
            yield break;
        }

        yield return root; // Include the starting node itself

        if (root is JsonObject obj)
        {
            foreach (var property in obj)
            {
                foreach (var descendant in property.Value.DescendantsAndSelf())
                {
                    yield return descendant;
                }
            }
        }
        else if (root is JsonArray arr)
        {
            foreach (var item in arr)
            {
                foreach (var descendant in item.DescendantsAndSelf())
                {
                    yield return descendant;
                }
            }
        }
    }
}