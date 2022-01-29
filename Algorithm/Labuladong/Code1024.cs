namespace Labuladong;
public class Code1024
{
    public static void Exection()
    {
        var intervals = new int[][] {
            new int[] { 0, 2 },
            new int[] { 4, 6 },
            new int[] { 8, 10 },
            new int[] { 1, 9 },
            new int[] { 1, 5 },
            new int[] { 5, 9 },};
        var t = 10;
        var result = GetInterval(intervals, t);
        Console.WriteLine(result);
    }

    private static int GetInterval(int[][] intervals, int t)
    {
        var list = new List<int[]>();
        var tmp = intervals.ToList().OrderBy(o => o[0]).ThenByDescending(o => o[1]).ToList();
        var first = tmp.First();
        if (first[0] != 0)
        {
            return -1;
        }
        list.Add(first);
        for (int i = 1; i < tmp.Count; i++)
        {
            var pre = list.Last();
            var curr = tmp[i];
            if (curr[0] > pre[1])
            {
                return -1;
            }
            if (curr[1] > pre[1])
            {
                list.Add(curr);
            }
        }
        var last = list.Last();
        if (last[1] != t)
        {
            return -1;
        }
        return list.Count;
    }
}