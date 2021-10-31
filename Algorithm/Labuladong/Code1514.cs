namespace Labuladong;
public class Code1514
{
    public static void Exection()
    {
        var n = 3;
        var edges = new int[][]{
            new int[]{0,1},
            new int[]{1,2},
            new int[]{0,2},
        };
        var succProb = new double[] { 0.5, 0.5, 0.2 };
        var start = 0;
        var end = 2;
        var result = MaxProbability(n, edges, succProb, start, end);
        Console.WriteLine(result);
    }

    public static double MaxProbability(int n, int[][] edges, double[] succProb, int start, int end)
    {
        List<double[]>[] graph = new List<double[]>[n];
        for (int i = 0; i < n; i++)
        {
            graph[i] = new List<double[]>();
        }
        // 构造邻接表结构表示图
        for (int i = 0; i < edges.Length; i++)
        {
            int from = edges[i][0];
            int to = edges[i][1];
            double weight = succProb[i];
            // 无向图就是双向图；先把 int 统一转成 double，待会再转回来
            graph[from].Add(new double[] { (double)to, weight });
            graph[to].Add(new double[] { (double)from, weight });
        }

        return Dijkstra(start, end, graph);
    }

    public class State
    {
        // 图节点的 id
        public int id { get; set; }
        // 从 start 节点到达当前节点的概率
        public double probFromStart { get; set; }


        public State(int id, double probFromStart)
        {
            this.id = id;
            this.probFromStart = probFromStart;
        }


    }

    public class DoubleComparer : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            if (x == y)
            {
                return 0;
            }
            if (x > y)
            {
                return -1;
            }
            return 1;
        }
    }

    public static double Dijkstra(int start, int end, List<double[]>[] graph)
    {
        // 定义：probTo[i] 的值就是节点 start 到达节点 i 的最大概率
        double[] probTo = new double[graph.Length];
        // dp table 初始化为一个取不到的最小值
        Array.Fill(probTo, -1);
        // base case，start 到 start 的概率就是 1
        probTo[start] = 1;

        // 优先级队列，probFromStart 较"大"的排在前面
        var pq = new PriorityQueue<State, double>(new DoubleComparer());
        // 从起点 start 开始进行 BFS
        pq.Enqueue(new State(start, 1), 1);

        while (pq.Count > 0)
        {
            State curState = pq.Dequeue();
            int curNodeID = curState.id;
            double curProbFromStart = curState.probFromStart;

            // 遇到终点提前返回
            if (curNodeID == end)
            {
                return curProbFromStart;
            }

            if (curProbFromStart < probTo[curNodeID])
            {
                // 已经有一条概率更大的路径到达 curNode 节点了
                continue;
            }
            // 将 curNode 的相邻节点装入队列
            foreach (double[] neighbor in graph[curNodeID])
            {
                int nextNodeID = (int)neighbor[0];
                // 看看从 curNode 达到 nextNode 的概率是否会更大
                double probToNextNode = probTo[curNodeID] * neighbor[1];
                if (probTo[nextNodeID] < probToNextNode)
                {
                    probTo[nextNodeID] = probToNextNode;
                    pq.Enqueue(new State(nextNodeID, probToNextNode), probToNextNode);
                }
            }
        }
        // 如果到达这里，说明从 start 开始无法到达 end，返回 0
        return 0.0;
    }
}