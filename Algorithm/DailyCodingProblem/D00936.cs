using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{


    /// <summary>
    /// Given a binary search tree and a range [a, b] (inclusive), return the sum of the elements of the binary search tree within the range.
    /// For example, given the following tree:
    ///      5
    ///     / \
    ///    3   8
    ///   / \ / \
    ///  2  4 6  10
    /// and the range[4, 9], return 23 (5 + 4 + 6 + 8).
    /// </summary>
    public class D00936
    {
        public class D00936Tree
        {
            public int Value { get; set; }
            public D00936Tree Left { get; set; }
            public D00936Tree Right { get; set; }
        }
        public static void Exection()
        {
            var tree = new D00936Tree
            {
                Value = 5,
                Left = new D00936Tree
                {
                    Value = 3,
                    Left = new D00936Tree { Value = 2, },
                    Right = new D00936Tree { Value = 4, },
                },
                Right = new D00936Tree
                {
                    Value = 8,
                    Left = new D00936Tree { Value = 6, },
                    Right = new D00936Tree { Value = 10, },
                },
            };

            var range = new int[2] { 4, 9 };
            int sum = Exection(tree, range);
            Console.WriteLine(sum);
        }

        private static int Exection(D00936Tree tree, int[] range)
        {
            if (tree == null)
            {
                return 0;
            }
            var left = Exection(tree.Left, range);
            var right = Exection(tree.Right, range);

            return left + right + (tree.Value >= range[0] && tree.Value <= range[1] ? tree.Value : 0);
        }
    }
}
