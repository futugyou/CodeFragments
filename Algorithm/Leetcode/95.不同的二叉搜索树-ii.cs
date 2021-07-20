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
        nodes = Build(1, n);
        return nodes;
    }

    private IList<TreeNode> Build(int start, int end)
    {
        IList<TreeNode> nodes = new List<TreeNode>();
        if (start > end)
        {
            nodes.Add(null);
            return nodes;
        }

        for (int i = start; i <= end; i++)
        {
            var leftnodes = Build(start, i - 1);
            var rightnodes = Build(i + 1, end);
            foreach (var left in leftnodes)
            {
                foreach (var right in rightnodes)
                {
                    TreeNode root = new TreeNode(start);
                    root.left = left;
                    root.right = right;
                    nodes.Add(root);
                }
            }
        }
        return nodes;
    }
}
// @lc code=end

