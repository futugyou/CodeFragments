using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// You are given a circular lock with three wheels, each of which display the numbers 0 through 9 in order. 
    /// Each of these wheels rotate clockwise and counterclockwise.
    /// In addition, the lock has a certain number of "dead ends", meaning that if you turn the wheels to one of these combinations, 
    /// the lock becomes stuck in that state and cannot be opened.
    /// Let us consider a "move" to be a rotation of a single wheel by one digit, in either direction. 
    /// Given a lock initially set to 000, a target combination, and a list of dead ends, 
    /// write a function that returns the minimum number of moves required to reach the target state, or None if this is impossible.
    /// </summary>
    public class D00938
    {
        public static void Exection()
        {
            var start = "000";
            var target = "678";
            var deads = new List<string> { "123", "345", "567" };
            HashSet<string> visited = new HashSet<string>();
            Queue<string> q = new Queue<string>();
            q.Enqueue(start);
            var step = 0;
            while (q.Count > 0)
            {
                var count = q.Count;
                for (int i = 0; i < count; i++)
                {
                    var t = q.Dequeue();
                    visited.Add(t);
                    if (t == target)
                    {
                        Console.WriteLine(step);
                        return;
                    }
                    if (deads.Contains(t))
                    {
                        continue;
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        var up = Step(q, t, j, true);
                        var down = Step(q, t, j, false);
                        if (!deads.Contains(up) && !visited.Contains(up))
                        {
                            q.Enqueue(up);
                        }
                        if (!deads.Contains(down) && !visited.Contains(down))
                        {
                            q.Enqueue(down);
                        }
                    }
                }
                step++;
            }
            Console.WriteLine(step);
        }

        private static string Step(Queue<string> q, string t, int j, bool dir)
        {
            var tmp = t.ToCharArray();
            if (dir)
            {
                if (tmp[j] == '9')
                {
                    tmp[j] = '0';
                }
                else
                {
                    tmp[j] = (char)(tmp[j] + 1);
                }
            }
            else
            {
                if (tmp[j] == '0')
                {
                    tmp[j] = '9';
                }
                else
                {
                    tmp[j] = (char)(tmp[j] - 1);
                }
            }
            return string.Join("", tmp);
        }
    }
}
