namespace DailyCodingProblem;

/// <summary>
/// Given the root of a binary search tree, and a target K, return two nodes in the tree whose sum equals K.
/// For example, given the following tree and K of 20
///    10
///   /   \
///  5     15
///       /  \
///     11    15
/// Return the nodes 5 and 15
/// </summary>
public class D01020
{
    public static void Exection()
    {
        var num = 16;
        var tree = new Tree(10)
        {
            Left = new Tree(5),
            Right = new Tree(15)
            {
                Left = new Tree(11),
                Right = new Tree(15)
            }
        };
        if (!DoExec(tree, tree, num))
        {
            Console.WriteLine("no match");
        }
    }

    public static bool DoExec(Tree tree, Tree currNode, int num)
    {
        if (tree == null || currNode == null)
        {
            return false;
        }
        var curr = currNode.Value;
        var sub = num - curr;
        if (FindNode(sub, tree, currNode))
        {
            Console.WriteLine(sub);
            Console.WriteLine(curr);
            return true;
        }

        if (DoExec(tree, currNode.Left, num)) return true;
        if (DoExec(tree, currNode.Right, num)) return true;
        return false;
    }

    public static bool FindNode(int sub, Tree tree, Tree currNode)
    {
        if (tree == null)
        {
            return false;
        }
        if (sub == tree.Value && tree != currNode)
        {
            return true;
        }
        if (sub < tree.Value)
        {
            return FindNode(sub, tree.Left, currNode);
        }
        else
        {
            return FindNode(sub, tree.Right, currNode);
        }
    }

    public class Tree
    {
        public Tree(int n)
        {
            Value = n;
        }
        public int Value { get; set; }
        public Tree Left { get; set; }
        public Tree Right { get; set; }
    }
}