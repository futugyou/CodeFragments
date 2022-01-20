namespace Labuladong;

public class TrieSet
{
    private static TrieMap<object> map = new();
    public void Add(string key) => map.Put(key, new());
    public void Remove(string key) => map.Remove(key);
    public bool Contains(string key) => map.ContainsKey(key);
    public bool HasKeyWithPrefix(string prefix) => map.HasKeyWithPrefix(prefix);
    public string ShortestPrefixOf(string query) => map.ShortestPrefixOf(query);
    public string LongestPrefixOf(string query) => map.LongestPrefixOf(query);
    public List<string> KeysWithPrefix(string prefix) => map.KeysWithPrefix(prefix);
    public List<string> KeysWithPattern(string pattern) => map.KeysWithPattern(pattern);
    public bool HasKeyWithPattern(string pattern) => map.HasKeyWithPattern(pattern);
    public int Count() => map.Count();
}