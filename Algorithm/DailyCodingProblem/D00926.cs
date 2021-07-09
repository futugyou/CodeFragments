using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given the root of a binary tree, find the most frequent subtree sum. 
    ///     The subtree sum of a node is the sum of all values under a node, including the node itself.
    /// For example, given the following tree:
    ///   5
    ///  / \
    /// 2  -5
    /// Return 2 as it occurs twice: once as the left leaf, and once as the sum of 2 + 5 - 5.
    /// </summary>
    public class D00926
    {
        private class SumTree
        {
            public int Value { get; set; }
            public SumTree Left { get; set; }
            public SumTree Right { get; set; }
        }
        public static void Exection()
        {
            var tree = new SumTree()
            {
                Value = 5,
                Left = new SumTree() { Value = 2, Left = new SumTree { Value = 1 }, Right = new SumTree { Value = 3 } },
                Right = new SumTree() { Value = -5, Left = new SumTree { Value = 6 }, Right = new SumTree { Value = 6 } },
                //Left = new SumTree() { Value = 2 },
                //Right = new SumTree() { Value = -5 },
            };

            var index = 1;
            var dic = new Dictionary<int, int>();
            ExecSum(index, dic, tree);
            Console.WriteLine(dic.GroupBy(x => x.Value).Select(p => p.Count()).OrderByDescending(p => p).FirstOrDefault());
        }

        private static void ExecSum(int index, Dictionary<int, int> dic, SumTree tree)
        {
            if (tree == null)
            {
                return;
            }
            dic.Add(index, tree.Value);
            for (int i = index / 2; i >= 1; i = i / 2)
            {
                dic[i] = dic[i] + tree.Value;
            }
            ExecSum(index * 2, dic, tree.Left);
            ExecSum(index * 2 + 1, dic, tree.Right);
        }
    }
}
