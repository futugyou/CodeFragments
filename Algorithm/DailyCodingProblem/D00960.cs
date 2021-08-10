using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// You are given an array of nonnegative integers. 
    /// Let's say you start at the beginning of the array and are trying to advance to the end. 
    /// You can advance at most, the number of steps that you're currently on. 
    /// Determine whether you can get to the end of the array.
    /// 
    /// For example, given the array[1, 3, 1, 2, 0, 1], we can go from indices 0 -> 1 -> 3 -> 5, so return true.
    /// Given the array[1, 2, 1, 0, 0], we can't reach the end, so return false.
    /// </summary>
    public class D00960
    {
        public static void Exection()
        {
            int[] nums = { 1, 3, 1, 2, 0, 1 };
            //int[] nums = { 1, 2, 1, 0, 0 };
            if (nums[0] == 0)
            {
                Console.WriteLine(false);
                return;
            }
            for (int i = 1; i < nums.Length; i++)
            {
                var find = Find(nums, i);
                if (!find)
                {
                    Console.WriteLine(false);
                    return;
                }
            }
            Console.WriteLine(true);
        }

        private static bool Find(int[] nums, int i)
        {
            for (int j = 0; j < nums.Length; j++)
            {
                if (j < i && i <= j + nums[j])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
