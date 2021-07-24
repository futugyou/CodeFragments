using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// We say a number is sparse if there are no adjacent ones in its binary representation. 
    /// For example, 21 (10101) is sparse, but 22 (10110) is not. 
    /// For a given input N, find the smallest sparse number greater than or equal to N.
    /// Do this in faster than O(N log N) time.
    /// </summary>
    public class D00947
    {
        public static void Exection()
        {
            var n = 22;
            while (true)
            {
                var count = 0;
                var t = n;
                while (t > 0)
                {
                    if (t % 2 == 1)
                    {
                        count++;
                    }
                    else
                    {
                        count = 0;
                    }
                    if (count == 2)
                    {
                        break;
                    }
                    t = t >> 1;
                }
                if (count < 2)
                {
                    break;
                }
                n++;
            }
            Console.WriteLine(n);
        }
    }
}
