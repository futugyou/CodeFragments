using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Starting from 0 on a number line, you would like to make a series of jumps that lead to the integer N.
    /// On the ith jump, you may move exactly i places to the left or right.
    /// Find a path with the fewest number of jumps required to get from 0 to N.
    /// </summary>
    public class D00968
    {
        public static void Exection()
        {
            var target = 123;

            Queue<int> queue = new Queue<int>();
            List<List<int>> lists = new List<List<int>>();
            lists.Add(new List<int> { 0 });
            var index = 0;
            while (!lists.Any(p => p.Sum() == target))
            {
                index++;
                var raw = lists.ConvertAll(p => new List<int>(p));
                lists.ForEach(p => p.Add(index));
                raw.ForEach(p => p.Add(-index));
                // optimization: same sum value of each list can choose one, and use object instead of int can log sum value.
                lists.AddRange(raw);
            }
            var list = lists.FirstOrDefault(p => p.Sum() == target);
            Console.WriteLine(string.Join(",", list));
        }
    }
}
