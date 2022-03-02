namespace Labuladong;

public class Code0040
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 2, 3, 4, 4, 5 };
        var target = 7;
        Sub(nums, target);
        foreach (var item in result)
        {
            Console.WriteLine(string.Join(",", item));
        }
    }

    private static void Sub(int[] nums, int target)
    {
        var res = new List<int>();
        Array.Sort(nums);
        BackTrack(res, 0, nums, target);
    }
    private static int trackSum = 0;
    private static void BackTrack(List<int> res, int v, int[] nums, int target)
    {
        if (target == trackSum)
        {
            var t = new int[res.Count];
            res.CopyTo(0, t, 0, res.Count);
            result.Add(t.ToList());
            return;
        }
        if (trackSum > target)
        {
            return;
        }
        for (int i = v; i < nums.Length; i++)
        {
            if (i > v && nums[i] == nums[i - 1])
            {
                continue;
            }
            res.Add(nums[i]);
            trackSum += nums[i];
            BackTrack(res, i + 1, nums, target);
            res.RemoveAt(res.Count - 1);
            trackSum -= nums[i];
        }
    }

    private static List<List<int>> result = new();

}