namespace Labuladong;

public class Code0700
{
    public static void Exection()
    {
    }
    public static BaseTree SearchTree(BaseTree root, int target)
    {
        if (root == null)
        {
            return null;
        }
        if (root.Value > target)
        {
            return SearchTree(root.Left, target);
        }
        else if (root.Value < target)
        {
            return SearchTree(root.Right, target);
        }
        else
        {
            return root;
        }
    }
}
