namespace Labuladong;
public class Code0222
{
    public static void Exection()
    {

    }

    public static int CountTree(BaseTree root)
    {
        if (root == null)
        {
            return 0;
        }
        var left = root;
        var right = root;
        var lefthigh = 0;
        var righthigh = 0;
        while (left != null)
        {
            left = left.Left;
            lefthigh++;
        }
        while (right != null)
        {
            right = right.Right;
            righthigh++;
        }
        if (lefthigh == righthigh)
        {
            return (int)(Math.Pow(2, lefthigh) - 1);
        }
        return 1 + CountTree(root.Left) + CountTree(root.Right);
    }
}