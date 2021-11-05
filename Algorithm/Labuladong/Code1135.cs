
namespace Labuladong;

public class Code1135
{
    public static void Exection()
    {
        var n = 3;
        var conections = new int[][]{
            new int[]{1,2,5},
            new int[]{1,3,6},
            new int[]{2,3,1},
        };
        var uf = new UnionFind(n);
        var ordered = conections.OrderBy(a => a[2]).ToArray();
        var mst = 0;
        foreach (var item in ordered)
        {
            int q = item[0];
            int p = item[1];
            int w = item[2];
            if (uf.Connected(q, p))
            {
                continue;
            }
            uf.Union(p, q);
            mst += w;
        }
        // 0 not use
        Console.WriteLine(uf.Count() == 2 ? mst : -1);
    }
}