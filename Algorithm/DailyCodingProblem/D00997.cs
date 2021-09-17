using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given the root to a binary tree, 
    /// implement serialize(root), which serializes the tree into a string, 
    /// and deserialize(s), which deserializes the string back into the tree.
    /// </summary>
    public class D00997
    {
        public class Tree
        {
            public Tree(int v)
            {
                Value = v;
            }
            public int Value { get; private set; }
            public Tree Left { get; set; }
            public Tree Right { get; set; }

        } 
        public static string Serialize(Tree tree)
        {
            if (tree == null)
            {
                return "";
            }
            var queue = new Queue<Tree>();
            queue.Enqueue(tree);
            var popcount = 1;
            var end = true;
            var list = new List<string>();
            while (queue.Count > 0)
            {
                end = true;
                for (int i = 0; i < popcount; i++)
                {
                    var item = queue.Dequeue();
                    if (item != null)
                    {
                        end = false;
                        queue.Enqueue(item.Left);
                        queue.Enqueue(item.Right);
                        list.Add(item.Value.ToString());
                    }
                    else
                    {
                        queue.Enqueue(null);
                        queue.Enqueue(null);
                        list.Add("null");
                    }
                }
                if (end)
                {
                    break;
                }
                popcount = popcount * 2;
            }
            return String.Join(",", list);
        }
        public static Tree Deserialize(int index, string[] splites)
        {
            var s = splites[index];
            if (s == "null")
            {
                return null;
            }
            var root = new Tree(int.Parse(s));
            if (2 * index + 1 < splites.Length)
            {
                root.Left = Deserialize(2 * index + 1, splites);
            }
            if (2 * index + 2 < splites.Length)
            {
                root.Right = Deserialize(2 * index + 2, splites);
            }
            return root;
        }
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
            var s = Serialize(tree);
            Console.WriteLine(s);
            var splites = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (splites.Count() > 0)
            {
                var tr = Deserialize(0, splites);
                Show(tr);
            }

        }

        private static void Show(Tree tr)
        {
            if (tr==null)
            {
                return;
            }
            Console.WriteLine(tr.Value);
            Show(tr.Left);
            Show(tr.Right);
        }
    }
}
