namespace Labuladong;
public class Code0785
{
    public static void Exection()
    {
        var graph = new int[][] {
            new int[] { 1, 2, 3 },
            new int[] { 0, 2 },
            new int[] { 0, 1, 3 },
            new int[] { 0, 2 },
            };
        int n = graph.Length;
        color = new bool[n];
        visited = new bool[n];
        for (int i = 0; i < n; i++)
        {
            if (!visited[i])
            {
                //DFS(graph, i);
                BFS(graph, i);
            }
        }
        Console.WriteLine(ok);
    }
    private static bool ok = true;
    private static bool[] color;
    private static bool[] visited;
    private static void DFS(int[][] graph, int n)
    {
        if (!ok)
        {
            return;
        }
        visited[n] = true;
        foreach (var w in graph[n])
        {
            if (!visited[w])
            {
                color[w] = !color[n];
                DFS(graph, w);
            }
            else
            {
                if (color[n] == color[w])
                {
                    ok = false;
                }
            }
        }
    }

    private static void BFS(int[][] graph, int n)
    {
        Queue<int> q = new Queue<int>();
        visited[n] = true;
        q.Enqueue(n);
        while (q.Any() && ok)
        {
            var v = q.Dequeue();
            foreach (var w in graph[v])
            {
                if (!visited[w])
                {
                    color[w] = !color[v];
                    visited[w] = true;
                    q.Enqueue(w);
                }
                else
                {
                    if (color[v] == color[w])
                    {
                        ok = false;
                    }
                }
            }
        }
    }
}