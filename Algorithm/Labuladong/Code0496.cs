namespace Labuladong;
public class Code0496
{
    public static void Exection()
    {
        var nums = new int[] { 2, 1, 2, 4, 3 };
        var r = NextBigger(nums);
        Console.WriteLine(string.Join(",", r));
    }

    public static int[] NextBigger(int[] nums)
    {
        var n = nums.Length;
        var result = new int[n];
        var stack = new Stack<int>(n);
        for (int i = n - 1; i >= 0; i--)
        {
            while (stack.Any() && stack.Peek() <= nums[i])
            {
                stack.Pop();
            }
            result[i] = stack.Any() ? stack.Peek() : -1;
            stack.Push(nums[i]);
        }
        return result;
    }
}