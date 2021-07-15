using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given n numbers, find the greatest common denominator between them.
    /// For example, given the numbers[42, 56, 14], return 14. 
    /// </summary>
    public class D00931
    {
        public static void Exection()
        {
            var nums = new int[] { 42, 56, 14, 70 };
            int max = 0;
            bool choose = true;
            for (int i = 0; i < nums.Length; i++)
            {
                choose = true;
                for (int j = 0; j < nums.Length; j++)
                {
                    var mod = nums[j] % nums[i];
                    if (mod != 0 || nums[j] / nums[i] < 1)
                    {
                        choose = false;
                        break;
                    }
                }
                if (choose)
                {
                    max = Math.Max(max, nums[i]);
                }
            }
            Console.WriteLine(max);
        }
    }
}
