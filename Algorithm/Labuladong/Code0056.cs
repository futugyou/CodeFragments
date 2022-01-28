namespace Labuladong;
public class Code0056
{
    public static void Exection()
    {
        var intervals = new int[][] {
            new int[] { 1, 3 },
            new int[] { 2, 6 },
            new int[] { 8, 10 },
            new int[] { 9, 18 }};
        List<int[]> result = GetInterval(intervals);
        foreach (var nums in result)
        {
            Console.WriteLine(string.Join(",", nums));
        }

    }

    private static List<int[]> GetInterval(int[][] intervals)
    {
        List<int[]> res = new();
        var tmp = intervals.ToList().OrderBy(o => o[0]).ToList();
        res.Add(tmp.First());
        for (int i = 1; i < tmp.Count; i++)
        {
            var curr = tmp[i];
            var pre = res.Last();
            if (curr[0] <= pre[1])
            {
                pre[1] = Math.Max(curr[1], pre[1]);
            }
            else
            {
                res.Add(curr);
            }
        }
        return res;
    }
}
