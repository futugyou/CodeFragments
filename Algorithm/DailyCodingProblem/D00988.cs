using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a positive integer n, find the smallest number of squared integers which sum to n.
    /// For example, given n = 13, return 2 since 13 = 32 + 22 = 9 + 4.
    /// Given n = 27, return 3 since 27 = 32 + 32 + 32 = 9 + 9 + 9.
    /// </summary>
    public class D00988
    {
        public static List<List<int>> results = new List<List<int>>();
        public static void Exection()
        {
            var n = 91;
            var list = new List<int>();
            DoExec2(n, list);
            //DoExec(n, list);
            foreach (var item in results)
            {
                Console.WriteLine(string.Join(",", item));
            }
        }

        private static void DoExec2(int n, List<int> list)
        {
            while (n > 0)
            {
                int i = (int)Math.Sqrt(n);
                list.Add(i);
                n = n - i * i;
            }
            results.Add(list);
        }

        private static void DoExec(int n, List<int> list)
        {
            if (n == 0)
            {
                results.Add(list.ToArray().ToList());
                return;
            }
            for (int i = (int)Math.Sqrt(n); i >= 1; i--)
            {
                if (n >= i * i)
                {
                    list.Add(i);
                    DoExec(n - i * i, list);
                    list.Remove(i);
                }
            }
        }
    }
}
