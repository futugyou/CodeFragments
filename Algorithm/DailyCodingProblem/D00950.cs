using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// You are given a list of data entries that represent entries and exits of groups of people into a building. An entry looks like this:
    /// {"timestamp": 1526579928, count: 3, "type": "enter"}
    /// This means 3 people entered the building. An exit looks like this:
    /// { "timestamp": 1526580382, count: 2, "type": "exit"}
    /// This means that 2 people exited the building. timestamp is in Unix time.
    /// Find the busiest period in the building, that is, the time with the most people in the building. 
    /// Return it as a pair of (start, end) timestamps.You can assume the building always starts off and ends up empty, i.e. with 0 people inside.
    /// </summary>
    public class D00950
    {
        public static void Exection()
        {
            var nums = new int[,] { { 1, 0 }, { 2, 1 }, { 3, 3 }, { 4, -2 }, { 5, 5 }, { 6, -4 }, { 7, -3 }, { 8, 0 }, { 9, 3 }, { 10, 2 }, { 11, -5 }, { 12, 0 } };
            var dp = new int[nums.Length / 2 + 1];
            dp[0] = 0;
            var max = 0;
            var curr = 0;
            var start = 0;
            var end = 0;
            for (int i = 1; i < nums.Length / 2; i++)
            {
                var v = nums[i, 1];
                var index = nums[i, 0];
                curr += v;
                if (curr >= max)
                {
                    start = index;
                    end = start;
                    if (i + 1 < nums.Length / 2)
                    {
                        end = nums[i + 1, 0];
                    }
                }
                max = Math.Max(curr, max);
            }
            Console.WriteLine($"max:{max} ,start:{start}, end:{end}");
        }
    }
}
