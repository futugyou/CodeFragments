using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{

    /// <summary>
    /// There exists a staircase with N steps, and you can climb up either 1 or 2 steps at a time. 
    /// Given N, write a function that returns the number of unique ways you can climb the staircase. 
    /// The order of the steps matters.
    /// For example, if N is 4, then there are 5 unique ways:
    ///  1, 1, 1, 1
    ///  2, 1, 1
    ///  1, 2, 1
    ///  1, 1, 2
    ///  2, 2
    /// What if, instead of being able to climb 1 or 2 steps at a time, you could climb any number from a set of positive integers X? 
    /// For example, if X = {1, 3, 5}, you could climb 1, 3, or 5 steps at a time.
    /// </summary>
    public class D00990
    {
        private static List<List<int>> totalList = new List<List<int>>();
        public static void Exection()
        {
            var steps = new int[] { 1, 3, 5 };
            var count = 6;
            var list = new List<int>();
            DoExec(steps, count, list);
            foreach (var item in totalList)
            {
                Console.WriteLine(string.Join(",", item));
            }
        }

        private static void DoExec(int[] steps, int count, List<int> list)
        {
            if (count == 0)
            {
                totalList.Add(list.ToArray().ToList());
                return;
            }
            foreach (var step in steps)
            {
                if (count >= step)
                {
                    list.Add(step);
                    DoExec(steps, count - step, list);
                    list.Remove(step);
                }
            }
        }
    }
}
