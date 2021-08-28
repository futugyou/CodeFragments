using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given the mapping a = 1, b = 2, ... z = 26, and an encoded message, count the number of ways it can be decoded.
    /// For example, the message '111' would give 3, since it could be decoded as 'aaa', 'ka', and 'ak'.
    /// You can assume that the messages are decodable.For example, '001' is not allowed.
    /// </summary>
    public class D00973
    {
        private static int count = 0;
        public static void Exection()
        {//123124345
            var raw = "1221";
            Exec(raw);
            Console.WriteLine(count);
        }
        private static void Exec(string raw)
        {
            if (raw.Length <= 0)
            {
                count++;
                return;
            }
            Exec(new string(raw.Skip(1).ToArray()));
            if (int.Parse(raw.Substring(0, 1)) < 3 && raw.Length > 1 && int.Parse(raw.Substring(0, 2)) <= 26)
            {
                Exec(new string(raw.Skip(2).ToArray()));
            }
        }
    }
}
