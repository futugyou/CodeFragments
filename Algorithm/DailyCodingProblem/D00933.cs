using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// The sequence [0, 1, ..., N] has been jumbled, 
    /// and the only clue you have for its order is an array representing whether each number is larger or smaller than the last.
    /// Given this information, reconstruct an array that is consistent with it. 
    /// For example, given [None, +, +, -, +], you could return [1, 2, 3, 0, 4].
    /// </summary>
    public class D00933
    {
        public static void Exection()
        {
            string[] given = { "", "+", "+", "-", "-", "+" };
            int[] nums = Enumerable.Range(0, given.Length).ToArray();
            var lesscount = given.Count(p => p == "-");
            int[] result = new int[nums.Length];
            result[0] = nums[lesscount];
            var lessindex = 0;
            for (int i = 1; i < given.Length; i++)
            {
                if (given[i] == "+")
                {
                    lesscount++;
                    result[i] = nums[lesscount];
                }
                else
                {
                    result[i] = nums[lessindex];
                    lessindex++;
                }
            }

            Console.WriteLine(string.Join(" ,", result));
        }
    }
}
