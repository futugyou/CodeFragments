using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given the root to a binary search tree, find the second largest node in the tree.
    /// </summary>
    public class D00992
    {
        public class Tree
        {
            public int Value { get; set; }
            public Tree Left { get; set; }
            public Tree Right { get; set; }
            public Tree(int n)
            {
                Value = n;
            }
        }
        public static void Exection()
        {
            var tree = new Tree(1);
            var pq = new PriorityQueue<Tree, int>();
            DoExec(tree, pq);
            while (pq.Count > 0)
            {
                Console.WriteLine(pq.Dequeue().Value);
            }
        }

        private static void DoExec(Tree tree, PriorityQueue<Tree, int> pq)
        {
            if (tree == null)
            {
                return;
            }
            pq.Enqueue(tree, tree.Value);
            DoExec(tree.Left, pq);
            DoExec(tree.Right, pq);
        }
    }
}
