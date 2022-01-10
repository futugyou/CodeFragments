namespace Labuladong;

public class Code0230
{
    public static void Exection()
    {

    }
    private static int result = 0;
    private static int index = 0;

    private static void FindKNode(BaseTree root, int k)
    {
        if (root == null)
        {
            return;
        }
        FindKNode(root.Left, k);
        index++;
        if (index == k)
        {
            result = root.Value;
            return;
        }
        FindKNode(root.Right, k);
    }
}