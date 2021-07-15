using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a binary tree, determine whether or not it is height-balanced. 
    /// A height-balanced binary tree can be defined as one in which the heights of the two subtrees of any node never differ by more than one.
    /// </summary>
    public class D00935
    {
        public class D00935Tree
        {
            public int Value { get; set; }
            public D00935Tree Left { get; set; }
            public D00935Tree Right { get; set; }
        }
        public static void Exection()
        {
            var tree = new D00935Tree
            {
                Value = 1,
                Left = new D00935Tree
                {
                    Value = 2,
                    Left = new D00935Tree
                    {
                        Value = 4,
                        Left = new D00935Tree { Value = 5, },
                    },

                    Right = new D00935Tree { Value = 6, },
                },
                Right = new D00935Tree
                {
                    Value = 3,
                    Left = new D00935Tree { Value = 7, }
                },
            };

            int result = ExecHeight(tree);
            Console.WriteLine(result == -1 ? "height NOT balance" : "height-balanced");
        }

        private static int ExecHeight(D00935Tree tree)
        {
            if (tree == null)
            {
                return 0;
            }
            int left = ExecHeight(tree.Left);
            if (left == -1) return -1;
            int right = ExecHeight(tree.Right);
            if (right == -1) return -1;
            if (Math.Abs(left - right) > 1)
            {
                return -1;
            }
            return 1 + Math.Max(left, right);
        }
    }
}
