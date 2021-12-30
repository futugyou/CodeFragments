namespace Labuladong;
public class Code0239
{
    public static void Exection()
    {
        var nums = new int[] { 1, 3, -1, -3, 5, 3, 6, 7 };
        var k = 3;
        int[] result = MaxInStep(nums, k);
        Console.WriteLine(string.Join(",", result));
    }

    public static int[] MaxInStep(int[] nums, int k)
    {
        var queue = new MonotonicQueue();
        var result = new List<int>();
        for (int i = 0; i < nums.Length; i++)
        {
            queue.Push(nums[i]);
            if (i >= k - 1)
            {
                result.Add(queue.Max());
                queue.Pop(nums[i - k + 1]);
            }
        }
        return result.ToArray();
    }
}