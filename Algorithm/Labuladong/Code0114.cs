
namespace Labuladong;
public class Code0114
{
    public static void Exection()
    {

    }

    public static void Flatten(BaseTree root)
    {
        if (root == null)
        {
            return;
        }
        Flatten(root.Left);
        Flatten(root.Right);

        var right = root.Right;
        var left = root.Left;

        root.Left = null;
        root.Right = left;

        var r = root;
        while (r.Right != null)
        {
            r = r.Right;
        }
        r.Right = right;
    }
}
