using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// A number is considered perfect if its digits sum up to exactly 10.
    /// Given a positive integer n, return the n-th perfect number.
    /// For example, given 1, you should return 19. Given 2, you should return 28.
    /// </summary>
    public class D00928
    {
        public static void Exection(int n = 10)
        {
            int count = 0;
            for (int i = 0; i < int.MaxValue; i++)
            {
                int sum = SumNo(i);
                if (sum == 10)
                {
                    count++;
                }
                if (count == n)
                {
                    Console.WriteLine(i);
                    break;
                }
            }
        }

        private static int SumNo(int i)
        {
            int sum = 0;
            while (i > 0)
            {
                sum += i % 10;
                i = i / 10;
            }
            return sum;
        }
    }
}
