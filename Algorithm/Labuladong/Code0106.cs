namespace Labuladong;

public class Code0106
{
    public static void Exection()
    {
        var postorder = new int[] { 5, 6, 7, 4, 2, 8, 9, 3, 1 };
        var inorder = new int[] { 5, 2, 6, 4, 7, 1, 8, 3, 9 };
        BaseTree tree = Builder(postorder, 0, postorder.Length - 1, inorder, 0, inorder.Length - 1);
    }

    private static BaseTree Builder(int[] postorder, int poststart, int postend, int[] inorder, int instart, int inend)
    {
        if (instart > inend)
        {
            return null;
        }
        var rootvalue = postorder[postend];
        var rootindex = 0;
        for (int i = instart; i <= inend; i++)
        {
            if (inorder[i] == rootvalue)
            {
                rootindex = i;
                break;
            }
        }
        var leftsize = rootindex - instart;
        BaseTree root = new BaseTree(rootvalue);
        root.Left = Builder(postorder, poststart, poststart + leftsize - 1, inorder, instart, rootindex - 1);
        root.Right = Builder(postorder, poststart + leftsize, postend - 1, inorder, rootindex + 1, inend);
        return root;
    }
}