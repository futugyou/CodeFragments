using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// You are given a 2 x N board, and instructed to completely cover the board with the following shapes:
    ///  Dominoes, or 2 x 1 rectangles.
    ///  Trominoes, or L-shapes.
    /// For example, if N = 4, here is one possible configuration, where A is a domino, and B and C are trominoes.
    ///  A B B C A C B
    ///  A B C C A C B
    ///  Given an integer N, determine in how many ways this task is possible.
    /// </summary>  1 1 3 5 7 13 23
    public class D00943
    {
        private static int count = 0;
        public static void Exection()
        {
            var n = 7;
            List<int> list = new List<int>();
            IniMethod(n, list);
            Console.WriteLine(count);
        }

        private static void IniMethod(int n, List<int> list)
        {
            if (n == 0)
            {
                count += (list.Any(p => p == 1) && !list.Any(p => p == 3) ? 1 : 0) + list.Count(p => p == 3) * 2;
                return;
            }
            foreach (var v in new int[] { 1, 3 })
            {
                if (n >= v)
                {
                    list.Add(v);
                    IniMethod(n - v, list);
                    list.RemoveAt(list.Count - 1);
                }
            }
        }
    }
}
