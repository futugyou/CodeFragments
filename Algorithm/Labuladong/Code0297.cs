using System.Text;
namespace Labuladong;

public class Code0297
{
    public const string SEP = ",";
    public const string NULL = "#";
    public static void Exection()
    {

    }
}

public interface ITreeSerialize
{
    string Serialize(BaseTree root);
    BaseTree Deserialize(string data);
}

public class FirstTreeSerialize : ITreeSerialize
{
    public BaseTree Deserialize(string data)
    {
        var nodes = new List<string>();
        foreach (var item in data.Split(Code0297.SEP))
        {
            nodes.Add(item);
        }
        return Deserialize(nodes);
    }

    public BaseTree Deserialize(List<string> nodes)
    {
        if (!nodes.Any())
        {
            return null;
        }

        var first = nodes.FirstOrDefault();
        nodes.RemoveAt(0);
        if (Code0297.NULL.Equals(first) || string.IsNullOrEmpty(first))
        {
            return null;
        }
        BaseTree root = new BaseTree(int.Parse(first));
        root.Left = Deserialize(nodes);
        root.Right = Deserialize(nodes);
        return root;
    }

    public string Serialize(BaseTree root)
    {
        var sb = new StringBuilder();
        Serialize(root, sb);
        return sb.ToString();
    }
    public void Serialize(BaseTree root, StringBuilder sb)
    {
        if (root == null)
        {
            sb.Append(Code0297.NULL).Append(Code0297.SEP);
            return;
        }
        sb.Append(root.Value).Append(Code0297.SEP);
        Serialize(root.Left, sb);
        Serialize(root.Right, sb);
    }
}
public class LastTreeSerialize : ITreeSerialize
{
    public BaseTree Deserialize(string data)
    {
        var nodes = new List<string>();
        foreach (var item in data.Split(Code0297.SEP))
        {
            nodes.Add(item);
        }
        return Deserialize(nodes);
    }

    public BaseTree Deserialize(List<string> nodes)
    {
        if (!nodes.Any())
        {
            return null;
        }
        var first = nodes.LastOrDefault();
        nodes.RemoveAt(nodes.Count - 1);
        if (Code0297.NULL.Equals(first) || string.IsNullOrEmpty(first))
        {
            return null;
        }
        BaseTree root = new BaseTree(int.Parse(first));
        root.Right = Deserialize(nodes);
        root.Left = Deserialize(nodes);
        return root;
    }

    public string Serialize(BaseTree root)
    {
        var sb = new StringBuilder();
        Serialize(root, sb);
        return sb.ToString();
    }
    public void Serialize(BaseTree root, StringBuilder sb)
    {
        if (root == null)
        {
            sb.Append(Code0297.NULL).Append(Code0297.SEP);
            return;
        }
        Serialize(root.Left, sb);
        Serialize(root.Right, sb);
        sb.Append(root.Value).Append(Code0297.SEP);
    }
}


public class LevelTreeSerialize : ITreeSerialize
{
    public BaseTree Deserialize(string data)
    {
        if (string.IsNullOrEmpty(data)) { return null; }
        var nodes = data.Split(Code0297.SEP);
        BaseTree root = new BaseTree(int.Parse(nodes[0]));
        Queue<BaseTree> q = new Queue<BaseTree>();
        q.Enqueue(root);

        for (int i = 1; i < nodes.Length; i++)
        {
            var parent = q.Dequeue();
            var left = nodes[i];
            i++;
            if (string.IsNullOrEmpty(left))
            {
                parent.Left = null;
            }
            else
            {
                parent.Left = new BaseTree(int.Parse(left));
                q.Enqueue(parent.Left);
            }

            var right = nodes[i];
            i++;
            if (string.IsNullOrEmpty(right))
            {
                parent.Right = null;
            }
            else
            {
                parent.Right = new BaseTree(int.Parse(right));
                q.Enqueue(parent.Right);
            }
        }
        return root;
    }

    public string Serialize(BaseTree root)
    {
        if (root == null)
        {
            return "";
        }
        var sb = new StringBuilder();
        Queue<BaseTree> q = new Queue<BaseTree>();
        q.Enqueue(root);
        while (q.Any())
        {
            var cur = q.Dequeue();
            if (cur == null)
            {
                sb.Append(Code0297.NULL).Append(Code0297.SEP);
                continue;
            }
            sb.Append(cur.Value).Append(Code0297.SEP);
            q.Enqueue(cur.Left);
            q.Enqueue(cur.Right);
        }
        return sb.ToString();
    }
}