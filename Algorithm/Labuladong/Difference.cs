namespace Labuladong;

public class Difference
{
    private int[] diff;

    public Difference(int[] nums)
    {
        diff = new int[nums.Length];
        diff[0] = nums[0];
        for (int i = 1; i < nums.Length; i++)
        {
            diff[i] = nums[i] - nums[i - 1];
        }
    }

    public void Increment(int start, int end, int value)
    {
        diff[start] += value;
        if (end + 1 < diff.Length)
        {
            diff[end + 1] -= value;
        }
    }

    public int[] Result()
    {
        int[] res = new int[diff.Length];
        res[0] = diff[0];
        for (int i = 1; i < diff.Length; i++)
        {
            res[i] = res[i - 1] + diff[i];
        }
        return res;
    }
}