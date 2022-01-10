namespace Labuladong;

public class Code0701
{
    public static void Exection()
    {
    }
    public static BaseTree InsertTree(BaseTree root, int target)
    {
        if (root == null)
        {
            return new(target);
        }
        if (root.Value > target)
        {
            root.Left = InsertTree(root.Left, target);
        }
        else if (root.Value < target)
        {
            root.Right = InsertTree(root.Right, target);
        }
        return root;
    }
}