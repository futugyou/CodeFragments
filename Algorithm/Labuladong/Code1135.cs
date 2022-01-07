
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
        UnionFindMethod(conections, n);
        PrimMethod(conections, n);
    }

    private static void PrimMethod(int[][] conections, int n)
    {
        List<int[]>[] graph = BuildGraph(n, conections);
        var prim = new Prim(graph);
        if (!prim.AllConnected())
        {
            Console.WriteLine(-1);
        }
        else
        {
            var sum = prim.TotleWeight();
            Console.WriteLine(sum);
        }
    }

    private static List<int[]>[] BuildGraph(int n, int[][] conections)
    {
        var graph = new List<int[]>[n];
        for (int i = 0; i < n; i++)
        {
            graph[i] = new List<int[]>();
        }
        foreach (var conn in conections)
        {
            var from = conn[0] - 1;
            var to = conn[1] - 1;
            var weight = conn[2];
            graph[from].Add(new int[] { from, to, weight });
            graph[to].Add(new int[] { to, from, weight });
        }
        return graph;
    }

    private static void UnionFindMethod(int[][] conections, int n)
    {
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