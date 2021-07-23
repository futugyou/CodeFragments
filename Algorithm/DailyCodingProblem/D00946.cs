using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Pascal's triangle is a triangular array of integers constructed with the following formula:
    /// The first row consists of the number 1.
    /// For each subsequent row, each element is the sum of the numbers directly above it, on either side.
    /// For example, here are the first few rows:
    ///
    ///       1
    ///      1 1
    ///     1 2 1
    ///    1 3 3 1
    ///   1 4 6 4 1
    /// 1 5 10 10 5 1
    /// Given an input k, return the kth row of Pascal's triangle.
    /// Bonus: Can you do this using only O(k) space?
    /// </summary>
    public class D00946
    {
        public static void Exection()
        {
            int n = 7;
            var result = new int[n];
            if (n == 1)
            {
                result = new int[1] { 1 };
            }
            else if (n == 2)
            {
                result = new int[2] { 1, 1 };
            }
            else
            {
                result[0] = 1;
                result[1] = 1;
                for (int i = 3; i <= n; i++)
                {
                    var t = result[0];
                    for (int j = 1; j < i; j++)
                    {
                        if (i == j + 1)
                        {
                            result[j] = 1;
                        }
                        else
                        {
                            var tt = result[j];
                            result[j] = t + result[j];
                            t = tt;
                        }
                    }
                }
            }
            Console.WriteLine(string.Join(" ", result));
        }
    }
}
