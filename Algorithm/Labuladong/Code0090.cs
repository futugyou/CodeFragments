namespace Labuladong;

public class Code0090
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 2 };
        Sub(nums);
        foreach (var item in result)
        {
            Console.WriteLine(string.Join(",", item));
        }
    }

    private static void Sub(int[] nums)
    {
        var res = new List<int>();
        Array.Sort(nums);
        BackTrack(res, 0, nums);
    }

    private static void BackTrack(List<int> res, int v, int[] nums)
    {
        var t = new int[res.Count];
        res.CopyTo(0, t, 0, res.Count);
        result.Add(t.ToList());
        for (int i = v; i < nums.Length; i++)
        {
            if (i > v && nums[i] == nums[i - 1])
            {
                continue;
            }
            res.Add(nums[i]);
            BackTrack(res, i + 1, nums);
            res.RemoveAt(res.Count - 1);
        }
    }

    private static List<List<int>> result = new();

}