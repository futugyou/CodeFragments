namespace Labuladong;

public class Code0077
{
    public static void Exection()
    {
        var n = 4;
        var k = 2;
        Compose(n, k);
        foreach (var item in result)
        {
            Console.WriteLine(string.Join(",", item));
        }
    }

    private static void Compose(int n, int k)
    {
        var res = new List<int>();
        BackTrack(res, 1, n, k);
    }

    private static void BackTrack(List<int> res, int start, int n, int k)
    {
        if (res.Count == k)
        {
            var t = new int[k];
            res.CopyTo(0, t, 0, k);
            result.Add(t.ToList());
            return;
        }
        for (int i = start; i <= n; i++)
        {
            if (!res.Contains(i))
            {
                res.Add(i);
                BackTrack(res, i + 1, n, k);
                res.RemoveAt(res.Count - 1);
            }
        }
    }

    private static List<List<int>> result = new();
}