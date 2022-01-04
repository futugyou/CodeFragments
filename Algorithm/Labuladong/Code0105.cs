namespace Labuladong;

public class Code0105
{
    public static void Exection()
    {
        var preorder = new int[] { 1, 2, 5, 4, 6, 7, 3, 8, 9 };
        var inorder = new int[] { 5, 2, 6, 4, 7, 1, 8, 3, 9 };
        BaseTree tree = Builder(preorder, 0, preorder.Length - 1, inorder, 0, inorder.Length - 1);
    }

    private static BaseTree Builder(int[] preorder, int prestart, int preend, int[] inorder, int instart, int inend)
    {
        if (prestart > preend)
        {
            return null;
        }
        var rootvalue = preorder[prestart];
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
        root.Left = Builder(preorder, prestart + 1, prestart + leftsize, inorder, instart, rootindex - 1);
        root.Right = Builder(preorder, prestart + leftsize + 1, preend, inorder, rootindex + 1, inend);
        return root;
    }
}