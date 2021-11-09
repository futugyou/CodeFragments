namespace DailyCodingProblem;

/// <summary>
/// You are given an array of non-negative integers that represents a two-dimensional elevation map 
/// where each element is unit-width wall and the integer is the height. 
/// Suppose it will rain and all spots between two walls get filled up.
/// Compute how many units of water remain trapped on the map in O(N) time and O(1) space.
/// For example, given the input[2, 1, 2], we can hold 1 unit of water in the middle.
/// Given the input [3, 0, 1, 3, 0, 5], we can hold 3 units in the first index, 2 in the second, 
/// and 3 in the fourth index (we cannot hold 5 since it would run off to the left), so we can trap 8 units of water.
/// </summary>
public class D01041
{
    public static void Exection()
    {
        var nums = new int[] { 3, 0, 1, 3, 0, 5 };
        ExecOne(nums);
        ExecTwo(nums);
    }

    private static void ExecTwo(int[] nums)
    {
        var n = nums.Length;
        var leftmax = nums[0];
        var rightmax = nums[n - 1];
        var sum = 0;
        var left = 0;
        var right = n - 1;
        while (left <= right)
        {
            leftmax = Math.Max(leftmax, nums[left]);
            rightmax = Math.Max(rightmax, nums[right]);

            if (leftmax < rightmax)
            {
                sum += leftmax - nums[left];
                left++;
            }
            else
            {
                sum += right - nums[right];
                right--;
            }
        }

        Console.WriteLine(sum);
    }

    private static void ExecOne(int[] nums)
    {
        var leftmax = new int[nums.Length];
        var rightmax = new int[nums.Length];
        leftmax[0] = nums[0];
        rightmax[nums.Length - 1] = nums[nums.Length - 1];
        for (int i = 1; i < nums.Length; i++)
        {
            leftmax[i] = Math.Max(nums[i], leftmax[i - 1]);
        }
        for (int i = nums.Length - 2; i >= 0; i--)
        {
            rightmax[i] = Math.Max(nums[i], rightmax[i + 1]);
        }
        var sum = 0;
        for (int i = 0; i < nums.Length; i++)
        {
            sum += Math.Min(leftmax[i], rightmax[i]) - nums[i];
        }
        Console.WriteLine(sum);
    }
}