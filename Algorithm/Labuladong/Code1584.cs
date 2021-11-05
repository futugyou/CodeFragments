
namespace Labuladong;

public class Code1584
{
    public static void Exection()
    {
        var points = new int[][]{
            new int[]{0,0},
            new int[]{2,2},
            new int[]{3,10},
            new int[]{5,2},
            new int[]{7,0},
        };
        var n = points.Length;
        var edges = new List<int[]>();
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                int xi = points[i][0];
                int xj = points[j][0];
                int yi = points[i][1];
                int yj = points[j][1];
                edges.Add(new int[3] { i, j, Math.Abs(xi - xj) + Math.Abs(yi - yj) });
            }
        }
        var edgesOrdered = edges.OrderBy(a => a[2]).ToList();
        int mst = 0;
        var uf = new UnionFind(n);
        foreach (var item in edgesOrdered)
        {
            var p = item[0];
            var q = item[1];
            var w = item[2];
            if (uf.Connected(p, q))
            {
                continue;
            }
            mst += w;
            uf.Union(p, q);
        }
        Console.WriteLine(mst);
    }
}