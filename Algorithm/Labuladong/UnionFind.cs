namespace Labuladong;

public class UnionFind
{
    private int count;
    private int[] parent;
    private int[] size;

    public UnionFind(int n)
    {
        count = n;
        parent = new int[n];
        size = new int[n];
        for (int i = 0; i < n; i++)
        {
            parent[i] = i;
            size[1] = 1;
        }
    }

    public void Union(int p, int q)
    {
        int rootp = FindRoot(p);
        int rootq = FindRoot(q);
        if (rootq == rootp)
        {
            return;
        }
        if (size[rootp] > size[rootq])
        {
            parent[rootq] = rootp;
            size[rootp] += size[rootq];
        }
        else
        {
            parent[rootp] = rootq;
            size[rootq] += size[rootp];
        }
        count--;
    }

    public int FindRoot(int x)
    {
        while (parent[x] != x)
        {
            // zip the tree, draw a gif your will got it.
            parent[x] = parent[parent[x]];
            x = parent[x];
        }
        return x;
    }

    public bool Connected(int p, int q)
    {
        int rootp = FindRoot(p);
        int rootq = FindRoot(q);
        return rootp == rootq;
    }

    public int Count() => count;
}