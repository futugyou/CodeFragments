using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    public class D00924
    {
        /// <summary>
        /// Given an array of integers out of order, determine the bounds of the smallest window that must be sorted in order for the entire array to be sorted. 
        /// For example, given [3, 7, 5, 6, 9], you should return (1, 3).
        /// </summary>
        /// <param name="n"></param>
        public static void Exection(int[] n)
        {
            int left = 0;
            int right = n.Length - 1;
            for (int i = 0; i < n.Length; i++)
            {
                var t = n[i];
                for (int j = i + 1; j < n.Length; j++)
                {
                    if (n[i] > n[j])
                    {
                        t = n[j];
                    }
                }
                if (t != n[i])
                {
                    left = i;
                    break;
                }
            }

            for (int i = n.Length - 1; i > 0; i--)
            {
                var t = n[i];
                for (int j = i - 1; j > 0; j--)
                {
                    if (n[i] < n[j])
                    {
                        t = n[j];
                    }
                }
                if (t != n[i])
                {
                    right = i;
                    break;
                }
            }
            Console.WriteLine($"({left} , {right})");
        }
    }
}
