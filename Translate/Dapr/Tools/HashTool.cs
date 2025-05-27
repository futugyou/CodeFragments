
namespace Tools;

public static class HashTool
{
    public static uint Fnv1aHash32(string input)
    {
        const uint FNV_OFFSET_BASIS = 2166136261;
        const uint FNV_PRIME = 16777619;

        uint hash = FNV_OFFSET_BASIS;
        foreach (char c in input)
        {
            hash ^= c;
            hash *= FNV_PRIME;
        }

        return hash;
    }
}