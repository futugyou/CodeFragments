using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{

    /// <summary>
    /// Suppose an arithmetic expression is given as a binary tree. Each leaf is an integer and each internal node is one of '+', '−', '∗', or '/'.
    ///    Given the root to such a tree, write a function to evaluate it.
    ///    For example, given the following tree:
    ///     *
    ///    / \
    ///   +    +
    ///  / \  / \
    /// 3  2  4  5
    /// You should return 45, as it is (3 + 2) * (4 + 5).
    /// </summary>
    public class D00955
    {
        public static void Exection()
        {
            var head = new BinaryTree
            {
                Option = "*",
                Left = new BinaryTree
                {
                    Option = "+",
                    Left = new BinaryTree { Value = 3 },
                    Right = new BinaryTree { Value = 2 },
                },
                Right = new BinaryTree
                {
                    Option = "+",
                    Left = new BinaryTree { Value = 4 },
                    Right = new BinaryTree { Value = 5 },
                }
            };
            Console.WriteLine(Exec(head));
        }

        private static int Exec(BinaryTree head)
        {
            if (head == null)
            {
                return 0;
            }
            if (head.Left == null && head.Right == null)
            {
                return head.Value;
            }
            var left = Exec(head.Left);
            var right = Exec(head.Right);
            switch (head.Option)
            {
                case "+":
                    return left + right;
                case "-":
                    return left - right;
                case "*":
                    return left * right;
                case "/":
                    return left / (right == 0 ? 1 : right);
                default:
                    break;
            }
            return 0;
        }

        private class BinaryTree
        {
            public BinaryTree Left { get; set; }
            public BinaryTree Right { get; set; }
            public int Value { get; set; }
            public string Option { get; set; }
        }
    }
}
