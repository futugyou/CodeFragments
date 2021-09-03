using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a binary tree, return all paths from the root to leaves.    ///
    ///  For example, given the tree:
    ///      1
    ///     / \
    ///    2   3
    ///       / \
    ///      4   5
    /// Return[[1, 2], [1, 3, 4], [1, 3, 5]].
    /// </summary>
    public class D00983
    {
        public static void Exection()
        {
            var tree = new Tree(1) { Left = new Tree(2) { Left = new Tree(6), Right = new Tree(7) }, Right = new Tree(3) { Left = new Tree(4), Right = new Tree(5) } };
            if (tree == null)
            {
                return;
            }
            var list = new List<int>();
            Show(tree, list);
        }

        private static void Show(Tree tree, List<int> list)
        {
            if (tree == null)
            {
                Console.WriteLine(string.Join(" ", list));
                return;
            }

            list.Add(tree.Value);
            if (tree.Left == null && tree.Right == null)
            {
                Show(null, list);
            }
            else
            {
                Show(tree.Left, list);
                Show(tree.Right, list);
            }
            list.RemoveAt(list.Count - 1);
        }

        class Tree
        {
            public Tree(int n) => Value = n;
            public int Value { get; set; }
            public Tree Left { get; set; }
            public Tree Right { get; set; }
        }
    }
}
