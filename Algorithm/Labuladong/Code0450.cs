namespace Labuladong;

public class Code0450
{
    public static void Exection()
    {
    }
    public static BaseTree DeleteKey(BaseTree root, int key)
    {
        if (root == null)
        {
            return null;
        }
        if (root.Value == key)
        {
            if (root.Left == null)
            {
                return root.Right;
            }
            if (root.Right == null)
            {
                return root.Left;
            }
            var min = GetMin(root.Right);
            root.Value = min.Value;
            root.Right = DeleteKey(root.Right, min.Value);
        }
        else if (root.Value > key)
        {
            root.Left = DeleteKey(root.Left, key);
        }
        else
        {
            root.Right = DeleteKey(root.Right, key);
        }
        return root;
    }

    public static BaseTree GetMin(BaseTree root)
    {
        while (root.Left != null)
        {
            root = root.Left;
        }
        return root;
    }
}