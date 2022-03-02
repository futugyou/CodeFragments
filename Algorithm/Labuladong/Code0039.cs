namespace Labuladong;

public class Code0039
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 3 };
        var target = 3;
        Permute(nums, target);
        foreach (var item in result)
        {
            Console.WriteLine(string.Join(",", item));
        }
    }
    private static List<List<int>> result = new();
    private static int trackSum = 0;
    public static void Permute(int[] nums, int target)
    {
        List<int> res = new();
        Backtrack(res, 0, nums, target);
    }

    private static void Backtrack(List<int> res, int start, int[] nums, int target)
    {
        if (target == trackSum)
        {
            var t = new int[res.Count];
            res.CopyTo(0, t, 0, res.Count);
            result.Add(t.ToList());
            return;
        }
        if (target < trackSum)
        {
            return;
        }
        for (int i = start; i < nums.Length; i++)
        {
            res.Add(nums[i]);
            trackSum += nums[i];
            Backtrack(res, i, nums, target);
            res.RemoveAt(res.Count - 1);
            trackSum -= nums[i];
        }
    }
}