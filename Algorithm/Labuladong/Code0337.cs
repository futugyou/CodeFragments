namespace Labuladong;

public class Code0337
{
    public static void Exection()
    {

    }

    private static Dictionary<BaseTree, int> mome = new Dictionary<BaseTree, int>();
    public static int Rob(BaseTree? root)
    {
        if (root == null)
        {
            return 0;
        }
        if (mome.ContainsKey(root))
        {
            return mome[root];
        }
        var rotthis = root.Value + (root.Left == null ? 0 : Rob(root.Left.Left) + Rob(root.Left.Right))
         + (root.Right == null ? 0 : Rob(root.Right.Left) + Rob(root.Right.Right));
        var notrotthis = Rob(root.Left) + Rob(root.Right);
        var res = Math.Max(rotthis, notrotthis);
        mome[root] = res;
        return res;
    }
}