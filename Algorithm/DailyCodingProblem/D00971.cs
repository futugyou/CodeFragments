using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given an N by N matrix, rotate it by 90 degrees clockwise.
    /// For example, given the following matrix:
    /// [[1, 2, 3],
    ///  [4, 5, 6],
    ///  [7, 8, 9]]
    /// you should return:
    /// [[7, 4, 1],
    ///  [8, 5, 2],
    ///  [9, 6, 3]]
    /// Follow-up: What if you couldn't use any extra space?
    /// </summary>
    public class D00971
    {
        public static void Exection()
        {
            int[,] nums = {
                {1,2,3},
                {4,5,6},
                {7,8,9},
            };
            int n = nums.GetLength(0);
            for (int i = 0; i < n / 2; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    int tmp = nums[i, j];
                    nums[i, j] = nums[n - j - 1, i];
                    nums[n - j - 1, i] = nums[n - i - 1, n - j - 1];
                    nums[n - i - 1, n - j - 1] = nums[j, n - i - 1];
                    nums[j, n - i - 1] = tmp;
                }
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(nums[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
