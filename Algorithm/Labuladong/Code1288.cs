namespace Labuladong;
public class Code1288
{
    public static void Exection()
    {
        var intervals = new int[][] {
            new int[] { 1, 6 },
            new int[] { 3, 7 },
            new int[] { 3, 8 },
            new int[] { 4, 9 },};

        var result = GetInterval(intervals);
        Console.WriteLine(result);
    }

    private static int GetInterval(int[][] intervals)
    {
        List<int[]> res = new();
        var tmp = intervals.ToList().OrderBy(o => o[0]).ThenByDescending(o => o[1]).ToList();
        res.Add(tmp.First());
        for (int i = 1; i < tmp.Count; i++)
        {
            var curr = tmp[i];
            var pre = res.Last();
            if (curr[1] <= pre[1])
            {
                pre[1] = Math.Max(curr[1], pre[1]);
            }
            else
            {
                res.Add(curr);
            }
        }
        return res.Count;
    }
}
