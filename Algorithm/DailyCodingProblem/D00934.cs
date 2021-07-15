using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a string, return the first recurring character in it, or null if there is no recurring character.
    /// For example, given the string "acbbac", return "b". Given the string "abcdef", return null.
    /// </summary>
    public class D00934
    {
        public static void Exection()
        {
            string given = "abcdef";
            List<char> read = new List<char>();
            for (int i = 0; i < given.Length; i++)
            {
                var str = given[i];
                if (read.Contains(str))
                {
                    Console.WriteLine(str);
                    return;
                }
                else
                {
                    read.Add(str);
                }
            }
            Console.WriteLine("null");
        }
    }
}
