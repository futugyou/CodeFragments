using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a string with repeated characters, rearrange the string so that no two adjacent characters are the same.
    /// If this is not possible, return None.
    /// For example, given "aaabbc", you could return "ababac". Given "aaab", return None.
    /// </summary>
    public class D00972
    {
        public static void Exection()
        {
            var raw = "aaabbbccc";
            if (raw.Length <= 1)
            {
                Console.WriteLine(raw);
                return;
            }
            var dics = new Dictionary<char, int>();
            //dics = raw.GroupBy(x => x, p => raw.Count(x => x == p)).Select(p => new { key = p.Key, count = p.Count() }).ToDictionary(p => p.key, p => p.count)
            for (int i = 0; i < raw.Length; i++)
            {
                if (dics.ContainsKey(raw[i]))
                {
                    dics[raw[i]]++;
                }
                else
                {
                    dics.Add(raw[i], 1);
                }
            }
            dics = dics.OrderByDescending(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
            if (dics.FirstOrDefault().Value > dics.Skip(1).Sum(p => p.Value) + 1)
            {
                Console.WriteLine("null");
                return;
            }
            char curr = '\0';
            char[] result = new char[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                if (curr == '\0')
                {
                    var now = dics.FirstOrDefault();
                    result[i] = now.Key;
                    dics[now.Key]--;
                    curr = now.Key;
                }
                else
                {
                    var now = dics.FirstOrDefault(p => p.Key != curr);
                    if (now.Key == '\0')
                    {
                        Console.WriteLine("null");
                        return;
                    }
                    else
                    {
                        result[i] = now.Key;
                        dics[now.Key]--;
                        curr = now.Key;
                    }
                }
                dics = dics.OrderByDescending(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
            }
            //for (int i = 1; i < chars.Length; i++)
            //{
            //    var index = i;
            //    while (chars[index] == chars[index - 1])
            //    {
            //        if (index == chars.Length - 1)
            //        {
            //            Console.WriteLine("null");
            //            return;
            //        }

            //    }
            //}

            Console.WriteLine(new string(result));
        }
    }
}
