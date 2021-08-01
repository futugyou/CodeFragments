using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{

    /// <summary>
    /// Given a list of integers, write a function that returns the largest sum of non-adjacent numbers. Numbers can be 0 or negative.
    /// For example, [2, 4, 6, 2, 5] should return 13, since we pick 2, 6, and 5. [5, 1, 1, 5] should return 10, since we pick 5 and 5.
    /// Follow-up: Can you do this in O(N) time and constant space?
    /// </summary>
    public class D00953
    {
        public static void Exection()
        {
            //int[] nums = new int[] { 2, 4, 6, 2, 5 };
            int[] nums = new int[] { 5, 1, 1, 5 };
            int[] dp = new int[nums.Length];
            dp[0] = nums[0];
            dp[1] = Math.Max(nums[0], nums[1]);
            for (int i = 2; i < nums.Length; i++)
            {
                dp[i] = Math.Max(dp[i - 2] + nums[i], dp[i - 1]);
            }
            Console.WriteLine(dp[nums.Length - 1]);
        }
    }
}
