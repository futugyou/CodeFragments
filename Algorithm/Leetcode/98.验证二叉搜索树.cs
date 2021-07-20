/*
 * @lc app=leetcode.cn id=98 lang=csharp
 *
 * [98] 验证二叉搜索树
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
    public bool IsValidBST(TreeNode root)
    {
        if (root == null)
        {
            return false;
        }
        // Build(root);
        // if (list.Count == 0 || list.Count == 1)
        // {
        //     return true;
        // }
        // for (int i = 1; i < list.Count; i++)
        // {
        //     if (list[i - 1] >= list[i])
        //     {
        //         return false;
        //     }
        // }
        // return true;
        return IsSubValidBST(root, long.MaxValue, long.MinValue);
    }
    List<long> list = new List<long>();
    public void Build(TreeNode root)
    {
        if (root == null)
        {
            return;
        }
        Build(root.left);
        list.Add(root.val);
        Build(root.right);

    }
    public bool IsSubValidBST(TreeNode root, long max, long min)
    {
        if (root == null)
        {
            return true;
        }

        if (root.val <= min || root.val >= max)
        {
            return false;
        }
        return IsSubValidBST(root.left, root.val, min) && IsSubValidBST(root.right, max, root.val);
    }
}
// @lc code=end

