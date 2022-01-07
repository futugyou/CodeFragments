namespace Labuladong;
public class Code0652
{
    public static void Exection()
    {

    }
    private static List<BaseTree> res = new();
    private static Dictionary<string, int> dic = new();
    public static string FindDuplicateSubtrees(BaseTree root)
    {
        if (root == null)
        {
            return "#";
        }
        var left = FindDuplicateSubtrees(root.Left);
        var right = FindDuplicateSubtrees(root.Right);
        var r = left + "," + right + "," + root.Value;

        if (dic.ContainsKey(r))
        {
            dic[r] = dic[r] + 1;
        }
        else
        {
            dic.Add(r, 1);
        }

        if (dic[r] == 2)
        {
            res.Add(root);
        }
        return r;
    }
}