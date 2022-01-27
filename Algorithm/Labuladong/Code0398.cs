namespace Labuladong;
public class Code0398
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 3, 3, 3 };
        var target = 3;
        for (int i = 0; i < 100; i++)
        {
            int result = GetRandomIndex(nums, target);
            Console.WriteLine(result);
        }
    }

    private static int GetRandomIndex(int[] nums, int target)
    {
        var index = -1;
        var count = 0;
        var rand = new Random();
        for (int i = 0; i < nums.Length; i++)
        {
            if (nums[i] != target)
            {
                continue;
            }
            count++;
            if (0 == rand.Next(count))
            {
                index = i;
            }
        }
        return index;
    }
}