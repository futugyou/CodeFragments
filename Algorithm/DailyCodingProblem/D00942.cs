using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a string of parentheses, write a function to compute the minimum number of parentheses to be removed to make the string valid 
    /// (i.e. each open parenthesis is eventually closed).
    /// For example, given the string "()())()", you should return 1. Given the string ")(", you should return 2, since we must remove all of them.
    /// </summary>
    public class D00942
    {
        public static void Exection()
        {
            var given = ")(()))((";
            int count = 0;
            int total = 0;
            for (int i = 0; i < given.Length; i++)
            {
                if (given[i] == '(')
                {
                    count++;
                }
                if (given[i] == ')')
                {
                    count--;
                }
                if (count < 0)
                {
                    total++;
                    count = 0;
                }
            }
            Console.WriteLine(total+count);
        }
    }
}
