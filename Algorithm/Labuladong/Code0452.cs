namespace Labuladong;
public class Code0452
{
    public static void Exection()
    {
        var intervals = new int[][] {
            new int[] { 10, 16 },
            new int[] { 2, 8 },
            new int[] { 1, 6 },
            new int[] { 7, 12 },};

        var result = GetInterval(intervals);
        Console.WriteLine(result);
    }

    private static int GetInterval(int[][] intervals)
    {
        var list = new List<int[]>();
        var tmp = intervals.ToList().OrderBy(o => o[0]).ThenByDescending(o => o[1]).ToList();
        // or 
        // var tmp = intervals.ToList().OrderBy(o => o[1]).ToList();
        list.Add(tmp.First());
        for (int i = 1; i < tmp.Count; i++)
        {
            var per = list.Last();
            var curr = tmp[i];
            if (curr[0] > per[1])
            {
                list.Add(curr);
            }
        }
        return list.Count;
    }
}