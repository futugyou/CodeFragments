namespace Labuladong;
public class Code0226
{
    public static void Exection()
    {

    }

    public static BaseTree InvertTree(BaseTree root)
    {
        if (root == null)
        {
            return null;
        }
        var t = root.Left;
        root.Left = root.Right;
        root.Right = t;

        InvertTree(root.Left);
        InvertTree(root.Right);
        return root;
    }
}