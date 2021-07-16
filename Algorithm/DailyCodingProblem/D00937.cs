using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// You're given a string consisting solely of (, ), and *. 
    /// * can represent either a (, ), or an empty string.
    /// Determine whether the parentheses are balanced.
    /// For example, (() * and(*) are balanced. )* ( is not balanced.
    /// </summary>
    public class D00937
    {
        public static void Exection()
        {
            //var target = "()*((*)**()(()))))))*(()";
            var target = "((*)*((**(*)";
            //var target = "(()*";
            //var target = "(*)";
            //var target = ")*(";
            if (target.FirstOrDefault() == ')' || target.LastOrDefault() == '(')
            {
                Console.WriteLine("not balanced");
                return;
            }
            Queue<int> q = new Queue<int>();
            q.Enqueue(0);
            for (int i = 0; i < target.Length; i++)
            {
                var count = q.Count;
                for (int j = 0; j < count; j++)
                {
                    var curr = q.Dequeue();
                    var t = target[i];
                    if (t == '(')
                    {
                        curr++;
                        q.Enqueue(curr);
                    }
                    if (t == ')')
                    {
                        curr--;
                        if (curr >= 0)
                        {
                            q.Enqueue(curr);
                        }
                    }
                    if (t == '*')
                    {
                        q.Enqueue(curr);
                        var add = curr + 1;
                        q.Enqueue(add);
                        var sub = curr - 1;
                        if (sub >= 0)
                        {
                            q.Enqueue(sub);
                        }
                    }
                }
            }
            if (q.Any(p => p == 0))
            {
                Console.WriteLine("balanced");
                return;
            }
            Console.WriteLine("not balanced");
        }
    }
}
