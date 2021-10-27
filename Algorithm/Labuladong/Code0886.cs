namespace Labuladong;
public class Code0886
{
    public static void Exection()
    {
        var dislikes = new int[][] {
            new int[] { 1, 2  },
            new int[] { 1, 3 },
            new int[] { 3, 4 },
            new int[] { 2, 4 },
            };

        int n = 5;// 0 1 2 3 4 form 0
        var graph = BuildGraph(dislikes, n);
        color = new bool[n];
        visited = new bool[n];
        for (int i = 0; i < n; i++)
        {
            if (!visited[i])
            {
                DFS(graph, i);
            }
        }
        Console.WriteLine(ok);
    }
    public static int[][] BuildGraph(int[][] dislikes, int n)
    {
        var graph = new int[n][];
        foreach (var item in dislikes)
        {
            var a = item[0];
            var b = item[1];
            graph[a].Append(b);
            graph[b].Append(a);
        }
        return graph;
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
}