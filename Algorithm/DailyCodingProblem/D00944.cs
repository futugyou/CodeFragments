using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given an integer, find the next permutation of it in absolute order. 
    /// For example, given 48975, the next permutation would be 49578.
    /// </summary>
    public class D00944
    {
        public static void Exection()
        {
            int num = 48975;
            numchars = (num + "").ToCharArray();
            int end = MaxNum();
            if (num == end)
            {
                Console.WriteLine(-1);
                return;
            }
            for (int i = num + 1; i < end; i++)
            {
                if (Check(i))
                {
                    Console.WriteLine(i);
                    return;
                }
            }
            Console.WriteLine(-1);
        }

        private static int MaxNum()
        {
            var ch = numchars.OrderByDescending(p => p);
            return int.Parse(string.Join("", ch));
        }
        private static char[] numchars;
        private static bool Check(int input)
        {
            var ch = (input + "").ToCharArray();
            if (numchars.Except(ch).Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
