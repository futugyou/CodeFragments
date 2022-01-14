namespace Labuladong;

public class Code0046
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 3 };
        Permute(nums);
        foreach (var item in result)
        {
            Console.WriteLine(string.Join(",", item));
        }
    }
    private static List<List<int>> result = new();
    public static void Permute(int[] nums)
    {
        List<int> res = new();
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
        foreach (var n in nums)
        {
            if (!res.Contains(n))
            {
                res.Add(n);
                Backtrack(res, nums);
                res.RemoveAt(res.Count - 1);
            }
        }
    }
}