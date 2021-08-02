using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{


    /// <summary>
    /// A unival tree (which stands for "universal value") is a tree where all nodes under it have the same value.
    /// Given the root to a binary tree, count the number of unival subtrees.
    /// For example, the following tree has 5 unival subtrees:
    ///   0
    ///  / \
    /// 1   0
    ///    / \
    ///   1   0
    ///  / \
    /// 1   1
    /// </summary>
    public class D00954
    {
        private static int sum = 0;
        public static void Exection()
        {
            var head = new BinaryTree
            {
                Value = 0,
                Left = new BinaryTree { Value = 1 },
                Right = new BinaryTree
                {
                    Value = 0,
                    Right = new BinaryTree { Value = 0 },
                    Left = new BinaryTree
                    {
                        Value = 1,
                        Left = new BinaryTree { Value = 1, },
                        Right = new BinaryTree { Value = 1, },
                    },
                }
            };

            Bfs(head);
            Console.WriteLine(sum);
        }

        private static bool Bfs(BinaryTree head)
        {
            if (head == null)
            {
                return true;
            }
            var left = Bfs(head.Left);
            var right = Bfs(head.Right);
            if (left && right)
            {
                if ((head.Left == null || head.Left.Value == head.Value) &&
                    (head.Right == null || head.Right.Value == head.Value))
                {
                    sum++;
                    return true;
                }
            }
            return false;
        }

        private class BinaryTree
        {
            public BinaryTree Left { get; set; }
            public BinaryTree Right { get; set; }
            public int Value { get; set; }
        }
    }


}
