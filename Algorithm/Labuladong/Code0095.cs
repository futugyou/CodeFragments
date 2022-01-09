
namespace Labuladong;

public class Code0095
{
    public static void Exection()
    {
        GenerateTrees(3);
    }


    public static List<BaseTree> GenerateTrees(int n)
    {
        if (n == 0)
        {
            return new();
        }
        return Build(1, n);
    }

    private static List<BaseTree> Build(int low, int high)
    {
        var res = new List<BaseTree>();
        if (low > high)
        {
            res.Add(null);
            return res;
        }
        for (int i = low; i <= high; i++)
        {
            var leftTree = Build(low, i - 1);
            var rightTree = Build(i + 1, high);
            foreach (var left in leftTree)
            {
                foreach (var right in rightTree)
                {
                    var root = new BaseTree(i);
                    root.Left = left;
                    root.Right = right;
                    res.Add(root);
                }
            }
        }
        return res;
    }
}