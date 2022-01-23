using System.Text;

namespace Labuladong;

public class TrieMap<T>
{
    private const int R = 256;
    private int count = 0;
    public int Count() => count;

    private TrieNode<T> root;

    private TrieNode<T>? GetNode(TrieNode<T> node, string key)
    {
        var p = node;
        foreach (var c in key)
        {
            if (p == null)
            {
                return null;
            }
            p = p.Children[c];
        }
        return p;
    }

    public T? Get(string key)
    {
        var node = GetNode(root, key);
        if (node == null)
        {
            return default(T);
        }
        return node.Value;
    }

    public bool ContainsKey(string key) => Get(key) != null;

    public bool HasKeyWithPrefix(string prefix) => GetNode(root, prefix) != null;

    public string ShortestPrefixOf(string query)
    {
        var p = root;
        for (int i = 0; i < query.Length; i++)
        {
            if (p == null)
            {
                return string.Empty;
            }
            if (p.Value != null)
            {
                return query.Substring(0, i);
            }
            var c = query[i];
            p = p.Children[c];
        }
        if (p != null && p.Value != null)
        {
            return query;
        }
        return string.Empty;
    }

    public string LongestPrefixOf(string query)
    {
        var p = root;
        var maxlenght = 0;
        for (int i = 0; i < query.Length; i++)
        {
            if (p == null)
            {
                break;
            }
            if (p.Value != null)
            {
                maxlenght = i;
            }
            var c = query[i];
            p = p.Children[c];
        }
        if (p != null && p.Value != null)
        {
            return query;
        }
        return query.Substring(0, maxlenght);
    }

    public List<string> KeysWithPrefix(string prefix)
    {
        var result = new List<string>();
        var node = GetNode(root, prefix);
        if (node != null)
        {
            Traverse(node, new StringBuilder(prefix), result);
        }
        return result;
    }

    private void Traverse(TrieNode<T> node, StringBuilder path, List<string> result)
    {
        if (node == null)
        {
            return;
        }
        if (node.Value != null)
        {
            result.Add(path.ToString());
        }
        for (int i = 0; i < R; i++)
        {
            var c = (char)i;
            var n = node.Children[c];
            if (n == null)
            {
                continue;
            }
            path.Append(c);
            Traverse(n, path, result);
            path.Remove(path.Length - 1, 1);
        }
    }

    public List<string> KeysWithPattern(string pattern)
    {
        var result = new List<string>();
        Traverse(root, new StringBuilder(), pattern, 0, result);
        return result;
    }

    private void Traverse(TrieNode<T> node, StringBuilder path, string pattern, int i, List<string> result)
    {
        if (node == null)
        {
            return;
        }
        if (i == pattern.Length)
        {
            if (node.Value != null)
            {
                result.Add(path.ToString());
            }
            return;
        }
        char c = pattern[i];
        if (c == '.')
        {
            for (int j = 0; j < R; j++)
            {
                var jj = (char)j;
                path.Append(jj);
                Traverse(node.Children[jj], path, pattern, i + 1, result);
                path.Remove(path.Length - 1, 1);
            }
        }
        else
        {
            path.Append(c);
            Traverse(node.Children[c], path, pattern, i + 1, result);
            path.Remove(path.Length - 1, 1);
        }
    }

    public bool HasKeyWithPattern(string pattern)
    {
        return HasKeyWithPattern(root, pattern, 0);
    }

    private bool HasKeyWithPattern(TrieNode<T> node, string pattern, int i)
    {
        if (node == null)
        {
            return false;
        }
        if (i == pattern.Length)
        {
            return node.Value != null;
        }
        char c = pattern[i];
        if (c == '.')
        {
            return HasKeyWithPattern(node.Children[c], pattern, i + 1);
        }
        for (int k = 0; k < R; k++)
        {
            var l = (char)k;
            if (HasKeyWithPattern(node.Children[l], pattern, i + 1))
            {
                return true;
            }
        }
        return false;
    }

    public void Put(string key, T val)
    {
        if (!ContainsKey(key))
        {
            count++;
        }
        root = Put(root, key, val, 0);
    }

    private TrieNode<T> Put(TrieNode<T> node, string key, T? val, int i)
    {
        if (node == null)
        {
            node = new TrieNode<T>();
        }
        if (i == key.Length)
        {
            node.Value = val;
            return node;
        }
        var c = key[i];
        node.Children[c] = Put(node.Children[c], key, val, i + 1);
        return node;
    }

    public void Remove(string key)
    {
        if (!ContainsKey(key))
        {
            return;
        }
        root = Remove(root, key, 0);
        count--;
    }

    private TrieNode<T> Remove(TrieNode<T> node, string key, int i)
    {
        if (node == null)
        {
            return null;
        }
        if (1 == key.Length)
        {
            node.Value = default;
        }
        else
        {
            var c = key[i];
            node.Children[c] = Remove(node.Children[c], key, i + 1);
        }
        if (node.Value != null)
        {
            return node;
        }
        for (int j = 0; j < R; j++)
        {
            var jj = (char)j;
            if (node.Children[jj] != null)
            {
                return node;
            }
        }
        return null;
    }

    private class TrieNode<V>
    {
        public V Value { get; set; }
        public TrieNode<V>[] Children { get; set; } = new TrieNode<V>[R];

    }
}