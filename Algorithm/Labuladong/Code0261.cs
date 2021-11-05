
namespace Labuladong;

public class Code0261
{
    public static void Exection()
    {
        var n = 5;
        var edges = new int[][]{
            new int[]{0,1},
            new int[]{0,2},
            new int[]{0,3},
            new int[]{1,4},
        };

        var f = ValedTree(n, edges);
        Console.WriteLine(f);
    }

    public static bool ValedTree(int n, int[][] edges)
    {
        var uf = new UF(n);
        foreach (var item in edges)
        {
            var p = item[0];
            var q = item[1];
            if (uf.Connected(q, p))
            {
                return false;
            }
            uf.Union(q, p);
        }
        return uf.Count() == 1;
    }

    private class UF
    {
        private int count;
        private int[] parents;
        private int[] sizes;
        public UF(int n)
        {
            count = n;
            parents = new int[n];
            sizes = new int[n];
            for (int i = 0; i < n; i++)
            {
                parents[i] = i;
                sizes[i] = 1;
            }
        }

        public bool Connected(int p, int q)
        {
            p = find(p);
            q = find(q);
            return q == p;
        }

        public void Union(int p, int q)
        {
            p = find(p);
            q = find(q);
            if (q == p)
            {
                return;
            }

            if (sizes[q] > sizes[p])
            {
                parents[p] = q;
                sizes[q] += sizes[p];
            }
            else
            {
                parents[q] = p;
                sizes[p] += sizes[q];
            }
            count--;
        }

        private int find(int n)
        {
            while (n != parents[n])
            {
                parents[n] = parents[parents[n]];
                n = parents[n];
            }
            return n;
        }
        public int Count() => count;
    }
}