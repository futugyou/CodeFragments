/*
 * @lc app=leetcode.cn id=283 lang=csharp
 *
 * [283] 移动零
 */

// @lc code=start
public class Solution
{
    public void MoveZeroes(int[] nums)
    {
        var index = 0;
        foreach (var item in nums)
        {
            if (item != 0)
            {
                nums[index] = item;
                index++;
            }
        }
        while (index < nums.Length)
        {
            nums[index] = 0;
            index++;
        }
    }
}
// @lc code=end

