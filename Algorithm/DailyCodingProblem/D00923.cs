using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    public class D00923
    {
        /// <summary>
        /// Given a 32-bit positive integer N, determine whether it is a power of four in faster than O(log N) time.
        /// </summary>
        public static bool Exection(int n)
        {
            if (n == 1)
            {
                return true;
            }
            return (n > 1) && (n & (n - 1)) == 0 && n % 3 == 1;
        }
    }
}
