namespace Labuladong;

public class RandomizedSet
{
    private List<int> list { get; set; } = new List<int>();
    private Dictionary<int, int> dic { get; set; } = new Dictionary<int, int>();
    private Random r = new Random();
    public bool Insert(int val)
    {
        if (dic.ContainsKey(val))
        {
            return false;
        }
        dic.Add(val, list.Count);
        list.Add(val);
        return true;
    }

    public bool Remove(int val)
    {
        if (!dic.ContainsKey(val))
        {
            return false;
        }
        var index = dic[val];
        var t = list.Last();
        dic[t] = index;
        dic.Remove(val);

        list[list.Count - 1] = list[index];
        list[index] = t;

        list.RemoveAt(list.Count - 1);
        return true;
    }

    public int GetRandom()
    {
        return list[r.Next() % list.Count];
    }
}