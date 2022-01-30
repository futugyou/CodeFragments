namespace Labuladong;
public class Code0514
{
    public static void Exection()
    {
        var ring = "godding";
        var key = "gd";
        var result = CountStep(ring, key);
        Console.WriteLine(result);
    }
    private static int[,] memo;
    private static Dictionary<char, List<int>> dic;
    private static int CountStep(string ring, string key)
    {
        var m = ring.Length;
        var n = key.Length;
        memo = new int[m, n];
        dic = new Dictionary<char, List<int>>();
        for (int i = 0; i < m; i++)
        {
            if (dic.ContainsKey(ring[i]))
            {
                dic[ring[i]].Add(i);
            }
            else
            {
                dic[ring[i]] = new List<int> { i };
            }
        }

        return Dp(ring, 0, key, 0);
    }

    private static int Dp(string ring, int i, string key, int j)
    {
        if (j == key.Length)
        {
            return 0;
        }
        if (memo[i, j] != 0)
        {
            return memo[i, j];
        }
        var n = ring.Length;
        int res = int.MaxValue;
        foreach (var index in dic[key[j]])
        {
            int step = Math.Abs(index - i);
            step = Math.Min(step, n - step);
            var sub = Dp(ring, index, key, j + 1);
            res = Math.Min(res, 1 + step + sub);
        }
        memo[i, j] = res;
        return res;
    }
}