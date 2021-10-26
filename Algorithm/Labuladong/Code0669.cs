namespace Labuladong;
public class Code0669
{
    public static void Exection()
    {
        var root = new TreeNode(3)
        {
            Left = new TreeNode(0)
            {
                Right = new TreeNode(2)
                {
                    Left = new TreeNode(1),
                },
            },
            Right = new TreeNode(4),
        };
        TrimBST(root, 1, 3);
        Display(root);
    }
    public static void Display(TreeNode root)
    {
        if (root == null)
        {
            return;
        }
        Console.Write(root.Val + " ");
        Display(root.Left);
        Display(root.Right);
    }
    public static TreeNode TrimBST(TreeNode root, int low, int high)
    {
        if (root == null) return null;
        if (root.Val > high) return TrimBST(root.Left, low, high);
        if (root.Val < low) return TrimBST(root.Right, low, high);
        root.Left = TrimBST(root.Left, low, high);
        root.Right = TrimBST(root.Right, low, high);
        return root;
    }

    public class TreeNode
    {
        public int Val;
        public TreeNode Left;
        public TreeNode Right;
        public TreeNode(int val = 0, TreeNode left = null, TreeNode right = null)
        {
            this.Val = val;
            this.Left = left;
            this.Right = right;
        }
    }
}