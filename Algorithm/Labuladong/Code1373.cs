namespace Labuladong;

public class Code1373
{
    public static void Exection()
    {

    }
    private static int Sum = 0;
    private static int[] MaxBST(BaseTree root)
    {
        if (root == null)
        {
            return new int[] { 1, int.MaxValue, int.MinValue, 0 };
        }
        var left = MaxBST(root.Left);
        var right = MaxBST(root.Right);
        var res = new int[4];
        if (left[0] == 1 && right[0] == 1
        && left[2] < root.Value
        && right[1] > root.Value)
        {
            res[0] = 1;
            res[1] = Math.Min(root.Value, left[1]);
            res[2] = Math.Max(root.Value, right[2]);
            res[3] = left[3] + right[3] + root.Value;
            Sum = Math.Max(Sum, res[3]);
        }
        return res;
    }
}