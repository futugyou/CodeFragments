namespace Labuladong;
public class Code0787
{
    public static void Exection()
    {
        var edges = new int[3][]{
            new int[]{ 0, 1, 100 },
            new int[]{ 1, 2, 100 },
            new int[]{ 0, 2, 500 },
        };
        var src = 0;
        var dst = 2;
        var k = 1;
        var n = 3;
        var result = CheapestPrice(edges, src, dst, k, n);
        Console.WriteLine(result);
    }
    private static int[,] memo;
    private static Dictionary<int, List<int[]>> path = new();
    private static int CheapestPrice(int[][] edges, int src, int dst, int k, int n)
    {
        memo = new int[n, k + 2];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < k + 2; j++)
            {
                memo[i, j] = int.MinValue;
            }
        }

        foreach (var item in edges)
        {
            int from = item[0];
            int to = item[1];
            int price = item[2];
            if (path.ContainsKey(to))
            {
                path[to].Add(new int[] { from, price });
            }
            else
            {
                path.Add(to, new List<int[]> { new int[] { from, price } });
            }
        }
        return Dp(src, dst, k + 1);
    }

    private static int Dp(int src, int dst, int k)
    {
        if (src == dst)
        {
            return 0;
        }
        if (k == 0)
        {
            return -1;
        }
        if (memo[dst, k] != int.MinValue)
        {
            return memo[dst, k];
        }
        var res = int.MaxValue;
        if (path.ContainsKey(dst))
        {
            foreach (var item in path[dst])
            {
                var from = item[0];
                var price = item[1];
                var sub = Dp(src, from, k - 1);
                if (sub != -1)
                {
                    res = Math.Min(res, sub + price);
                }
            }
        }
        memo[dst, k] = res == int.MaxValue ? -1 : res;
        return res;
    }
}
