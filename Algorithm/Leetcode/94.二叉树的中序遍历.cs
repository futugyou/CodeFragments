/*
 * @lc app=leetcode.cn id=94 lang=csharp
 *
 * [94] 二叉树的中序遍历
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
    private IList<int> result;
    public IList<int> InorderTraversal(TreeNode root)
    {
        result = new List<int>();
        Search(root);
        return result;
    }

    private void Search(TreeNode root)
    {
        if (root == null)
        {
            return;
        }
        Search(root.left);
        result.Add(root.val);
        Search(root.right);
    }
}
// @lc code=end

