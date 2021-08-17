using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// You are given an array of length n + 1 whose elements belong to the set {1, 2, ..., n}. 
    /// By the pigeonhole principle, there must be a duplicate. Find it in linear time and space.
    /// </summary>
    public class D00967
    {
        public static void Exection()
        {
            int[] test = { 1, 1, 2, 2, 3, 4, 5, 1, 2, 3, 4 };
            int[] raws = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var nums = new int[raws.Length + 1];
            for (int i = 0; i < test.Length; i++)
            {
                nums[test[i]] = 1 + nums[test[i]];
            }
            Console.WriteLine(string.Join(",", nums));
        }
    }
}
