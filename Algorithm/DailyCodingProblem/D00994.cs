using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Print the nodes in a binary tree level-wise. For example, the following should print 1, 2, 3, 4, 5.
    ///    1
    ///   / \
    ///  2   3
    ///     / \
    ///    4   5
    /// </summary>
    public class D00994
    {
        public static void Exection()
        {
            var tree = new Tree(1)
            {
                Left = new Tree(2) { Right = new Tree(9) { Left = new Tree(8), Right = new Tree(-1) }, },
                Right = new Tree(3)
                {
                    Left = new Tree(4),
                    Right = new Tree(5)
                },
            };

            DoExec(tree);
        }

        private static void DoExec(Tree tree)
        {
            var queue = new Queue<Tree>();
            queue.Enqueue(tree);
            while (queue.Count > 0)
            {
                var currCount = queue.Count;
                for (int i = 0; i < currCount; i++)
                {
                    var curr = queue.Dequeue();
                    Console.WriteLine(curr.Value);
                    if (curr.Left != null)
                    {
                        queue.Enqueue(curr.Left);
                    }
                    if (curr.Right != null)
                    {
                        queue.Enqueue(curr.Right);
                    }
                }
            }

        }

        private class Tree
        {
            public Tree(int v)
            {
                Value = v;
            }
            public int Value { get; private set; }
            public Tree Left { get; set; }
            public Tree Right { get; set; }
        }
    }
}
