namespace Labuladong;
public class Code0435
{
    public static void Exection()
    {
        var intervals = new int[][] {
            new int[] { 1, 6 },
            new int[] { 3, 7 },
            new int[] { 3, 8 },
            new int[] { 7, 9 },};

        var result = GetInterval(intervals);
        Console.WriteLine(result);
    }

    private static int GetInterval(int[][] intervals)
    {
        var list = new List<int[]>();
        var tmp = intervals.ToList().OrderBy(o => o[1]).ToList();
        list.Add(tmp.First());
        for (int i = 1; i < tmp.Count; i++)
        {
            var pre = list.Last();
            var curr = tmp[i];
            if (curr[0] > pre[1])
            {
                list.Add(curr);
            }
        }
        return list.Count;
    }
}