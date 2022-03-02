namespace Labuladong;

public class Code0047
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 2, 3, 3 };
        Permute(nums);
        foreach (var item in result)
        {
            Console.WriteLine(string.Join(",", item));
        }
    }
    private static List<List<int>> result = new();
    private static bool[] used;
    public static void Permute(int[] nums)
    {
        List<int> res = new();
        used = new bool[nums.Length];
        Array.Sort(nums);
        Backtrack(res, nums);
    }

    private static void Backtrack(List<int> res, int[] nums)
    {
        if (res.Count == nums.Length)
        {
            var t = new int[nums.Length];
            res.CopyTo(0, t, 0, nums.Length);
            result.Add(t.ToList());
            return;
        }
        for (int i = 0; i < nums.Length; i++)
        {
            if (used[i])
            {
                continue;
            }
            if (i > 0 && nums[i] == nums[i - 1] && !used[i - 1])
            {
                continue;
            }
            res.Add(nums[i]);
            used[i] = true;
            Backtrack(res, nums);
            res.RemoveAt(res.Count - 1);
            used[i] = false;
        }
    }
}