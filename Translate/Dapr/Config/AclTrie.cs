


namespace Config;

public class AccessControlListOperationAction
{
    public Dictionary<string, string> VerbActions { get; set; } = [];
    public string OperationName { get; set; } = "";
    public string OperationAction { get; set; } = "";
}

public class Trie
{
    private const char Separation = '/';
    private const string SingleStageWildcard = "/*";
    private const string MultiStageWildcard = "/**";

    TrieNode? Root { get; set; }
    public Trie()
    {
        Root = new TrieNode("/", new AccessControlListOperationAction());
    }

    public AccessControlListOperationAction? Search(string operation)
    {
        var node = Root!;
        var operationParts = operation.Split(Separation);
        var length = operationParts.Length;

        for (var index = 0; index < length; index++)
        {
            if (index == 0)
            {
                continue;
            }
            var ch = operationParts[index];
            var isEnd = index == length - 1;

            ch = Separation + ch;
            node = node!.FindSubNode(ch, isEnd);
            if (node == null)
            {
                return null;
            }

            if (node.Data != null)
            {
                if (!isEnd && node.Char.EndsWith(SingleStageWildcard) && !node.Char.EndsWith(MultiStageWildcard))
                {
                    continue;
                }
                return node.Data;
            }
            else if (isEnd)
            {
                node = node.FindSubNode(SingleStageWildcard, isEnd);
                if (node != null && node.Data != null)
                {
                    return node.Data;
                }
            }
        }
        return null;
    }

    public void PutOperationAction(string operation, AccessControlListOperationAction? data)
    {
        var operationParts = operation.Split(Separation);
        var length = operationParts.Length;

        TrieNode node = Root ?? new TrieNode("/", new AccessControlListOperationAction());
        for (var index = 0; index < length; index++)
        {
            if (index == 0)
            {
                continue;
            }
            var ch = operationParts[index];
            ch = Separation + ch;
            var subNode = FindNode(ch, node.SubNodes);
            
            if (null == subNode)
            {
                TrieNode? newNode;
                if (index == length - 1)
                {
                    newNode = new TrieNode(ch, data);
                }
                else
                {
                    newNode = new TrieNode(ch, null);
                }

                node.AddSubNode(newNode);
                node = newNode;
            }
            else if (index == length - 1)
            {
                subNode.Data ??= data;
            }
            else
            {
                node = subNode;
            }
        }
    }

    private static TrieNode? FindNode(string @char, List<TrieNode> nodes)
    {
        if (null == nodes || nodes.Count < 1)
        {
            return null;
        }

        foreach (var node in nodes)
        {
            if (node.Char == @char)
            {
                return node;
            }
        }

        return null;
    }

    private class TrieNode(string ch, AccessControlListOperationAction? data)
    {
        public string Char { get; set; } = ch;
        public AccessControlListOperationAction? Data { get; set; } = data;
        public List<TrieNode> SubNodes { get; set; } = [];

        public void AddSubNode(TrieNode newNode)
        {
            if (SubNodes == null)
            {
                SubNodes = [newNode];
            }
            else
            {
                SubNodes.Add(newNode);
            }
        }

        public TrieNode? FindSubNode(string target, bool isEnd)
        {
            if (SubNodes == null)
            {
                return null;
            }

            return FindNodeWithWildcard(target, SubNodes, isEnd);
        }

        private static TrieNode? FindNodeWithWildcard(string target, List<TrieNode> subNodes, bool isEnd)
        {
            if (subNodes == null || subNodes.Count < 1)
            {
                return null;
            }

            foreach (var node in subNodes)
            {
                if (node.Char == target)
                {
                    return node;
                }
                if (node.Char == SingleStageWildcard)
                {
                    if (isEnd)
                    {
                        return node;
                    }
                    continue;
                }
                if (node.Char == MultiStageWildcard)
                {
                    return node;
                }

                if (IsMatch(target, node.Char))
                {
                    return node;
                }
            }

            return null;
        }

        private static bool IsMatch(string target, string patten)
        {
            var tl = target.Length;
            var pl = patten.Length;

            var matchResults = new bool[tl + 1][];
            for (var i = 0; i <= tl; i++)
            {
                matchResults[i] = new bool[pl + 1];
            }
            matchResults[0][0] = true;
            for (var i = 1; i <= pl; i++)
            {
                if (patten[i - 1] == '*')
                {
                    matchResults[0][i] = true;
                }
                else
                {
                    break;
                }
            }

            for (var i = 1; i <= tl; i++)
            {
                for (var j = 1; j <= pl; j++)
                {
                    if (patten[j - 1] == '*')
                    {
                        matchResults[i][j] = matchResults[i][j - 1] || matchResults[i - 1][j];
                    }
                    else if (target[i - 1] == patten[j - 1])
                    {
                        matchResults[i][j] = matchResults[i - 1][j - 1];
                    }
                }
            }
            return matchResults[tl][pl];
        }
    }

}