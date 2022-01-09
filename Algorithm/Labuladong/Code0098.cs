namespace Labuladong;

public class Code0098
{
    public static void Exection()
    {
    }
    public bool IsValidBST(BaseTree root)
    {
        return IsValidBST(root, null, null);
    }

    private bool IsValidBST(BaseTree root, BaseTree min, BaseTree max)
    {
        if (root == null)
        {
            return true;
        }
        if (min != null && root.Value <= min.Value)
        {
            return false;
        }
        if (max != null && root.Value >= max.Value)
        {
            return false;
        }
        return IsValidBST(root.Left, min, root) && IsValidBST(root.Right, root, max);
    }
}