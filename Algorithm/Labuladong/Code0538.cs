namespace Labuladong;

public class Code0538
{
    public static void Exection()
    {

    }
    private static int sum = 0;

    private static void SumTree(BaseTree root)
    {
        if (root == null)
        {
            return;
        }
        SumTree(root.Right);
        sum += root.Value;
        root.Value = sum;
        SumTree(root.Left);
    }
}