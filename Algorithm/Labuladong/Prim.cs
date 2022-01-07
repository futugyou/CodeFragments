namespace Labuladong;
public class Prim
{
    private PriorityQueue<int[], int> queue;
    private bool[] inMST;
    private int weightSum = 0;
    private List<int[]>[] graph;

    public Prim(List<int[]>[] g)
    {
        graph = g;
        var n = g.Length;
        inMST = new bool[n];
        queue = new PriorityQueue<int[], int>(new GraphComparer());

        inMST[0] = true;
        Cut(0);
        while (queue.Count > 0)
        {
            var edge = queue.Dequeue();
            var to = edge[1];
            var weight = edge[2];
            if (inMST[to])
            {
                continue;
            }
            weightSum += weight;
            inMST[to] = true;
            Cut(to);
        }
    }

    private void Cut(int s)
    {
        foreach (var g in graph[s])
        {
            var from = g[0];
            var to = g[1];
            var weight = g[2];
            if (inMST[to])
            {
                continue;
            }
            queue.Enqueue(g, weight);
        }
    }

    public int TotleWeight() => weightSum;

    public bool AllConnected() => !inMST.Any(p => !p);
    private class GraphComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x > y)
            {
                return -1;
            }
            else if (x < y)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}