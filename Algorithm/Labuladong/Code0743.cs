namespace Labuladong;
public class Code0743
{
    public static void Exection()
    {
        int[][] times = new int[3][]{
            new int[]{2,1,1},
            new int[]{2,31,1},
            new int[]{3,4,1},
            };
        var n = 4;
        var k = 2;
        var r = NetworkDelayTime(times, n, k);
        Console.WriteLine(r);
    }

    public static int NetworkDelayTime(int[][] times, int n, int k)
    {
        var graph = new List<int[]>[n + 1];
        for (int i = 0; i < n + 1; i++)
        {
            graph[i] = new List<int[]>();
        }
        foreach (var item in times)
        {
            var from = item[0];
            var to = item[1];
            var weight = item[2];
            graph[from].Add(new int[] { to, weight });
        }
        int[] distTo = Dijkstra(k, graph);
        int res = 0;
        for (int i = 0; i < distTo.Length; i++)
        {
            if (distTo[i] == int.MaxValue)
            {
                return -1;
            }
            res = Math.Max(res, distTo[i]);
        }
        return res;
    }

    private static int[] Dijkstra(int k, List<int[]>[] graph)
    {
        int[] distto = new int[graph.Length];
        Array.Fill(distto, int.MaxValue);
        distto[k] = 0;
        var pq = new PriorityQueue<State, int>();
        pq.Enqueue(new State(k, 0), 0);
        while (pq.Count > 0)
        {
            var state = pq.Dequeue();
            var curNodeid = state.id;
            var curDistFromstart = state.distFromStart;
            if (curDistFromstart > distto[curNodeid])
            {
                continue;
            }
            foreach (var item in graph[curNodeid])
            {
                var id = item[0];
                var nextdistfromstart = item[1] + distto[curNodeid];
                if (nextdistfromstart < distto[id])
                {
                    distto[id] = nextdistfromstart;
                    pq.Enqueue(new State(id, nextdistfromstart), nextdistfromstart);
                }
            }
        }
        return distto;
    }
    class State
    {
        public int id { get; set; }
        public int distFromStart { get; set; }
        public State(int id, int distFromStart)
        {
            this.id = id;
            this.distFromStart = distFromStart;
        }
    }
}