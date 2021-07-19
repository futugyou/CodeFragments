/*
 * @lc app=leetcode.cn id=95 lang=csharp
 *
 * [95] 不同的二叉搜索树 II
 */

// @lc code=start
/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int val=0, TreeNode left=null, TreeNode right=null) {
 *         this.val = val;
 *         this.left = left;
 *         this.right = right;
 *     }
 * }
 */
public class Solution
{
    public IList<TreeNode> GenerateTrees(int n)
    {
        IList<TreeNode> nodes = new List<TreeNode>();
        IList<int> visited = new List<int>();
        Checked(nodes, visited, null, n);
        return nodes;
    }
    private bool Find(IList<TreeNode> nodes, TreeNode ro)
    {
        if (ro == null || nodes.Count == 0) return false;
        foreach (var item in nodes)
        {
            if (TreeEquals(item, ro))
            {
                return true;
            }
        }
        return false;
    }

    private bool TreeEquals(TreeNode left, TreeNode right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        if (left.val != right.val) return false;
        return TreeEquals(left.left, right.left) && TreeEquals(left.right, right.right);
    }
    private void Checked(IList<TreeNode> nodes, IList<int> visited, TreeNode ro, int n)
    {
        if (visited.Count == n)
        {
            var clone = Clone(ro);
            if (!Find(nodes, clone))
            {
                nodes.Add(clone);
            }
            return;
        }
        for (int i = 1; i <= n; i++)
        {
            if (visited.Contains(i))
            {
                continue;
            }
            TreeNode root = Add(ro, i);
            visited.Add(i);
            Checked(nodes, visited, root, n);
            Remove(root, i);
            visited.Remove(i);
        }
    }

    private TreeNode Add(TreeNode root, int value)
    {
        if (root == null)
        {
            return new TreeNode(value);
        }
        else if (root.val < value)
        {
            root.right = Add(root.right, value);
        }
        else
        {
            root.left = Add(root.left, value);
        }
        return root;
    }

    private TreeNode Remove(TreeNode root, int value)
    {
        if (root == null)
        {
            return null;
        }
        else if (root.val < value)
        {
            root.right = Remove(root.right, value);
        }
        else if (root.val > value)
        {
            root.left = Remove(root.left, value);
        }
        else
        {
            root = null;
        }
        return root;
    }

    private TreeNode Clone(TreeNode root)
    {
        if (root == null)
        {
            return null;
        }
        TreeNode newroot = new TreeNode(root.val);
        newroot.left = Clone(root.left);
        newroot.right = Clone(root.right);
        return newroot;
    }
}
// @lc code=end

