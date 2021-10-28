namespace Labuladong;
public class Code0797
{
    public static List<List<int>> result = new List<List<int>>();
    public static void Exection()
    {
        // 0, 1, 2,..., n - 1
        var graph = new int[][] {
            new int[] { 1, 2  },
            new int[] { 3 },
            new int[] { 3 },
            new int[] {  },
            };
        var path = new List<int>();
        Dfs(graph, 0, path);
        foreach (var item in result)
        {
            Console.WriteLine(string.Join(",", item));
        }
    }

    private static void Dfs(int[][] graph, int v, List<int> path)
    {
        var n = graph.Length;
        path.Add(v);
        if (v == n - 1)
        {
            var t = new int[path.Count];
            path.CopyTo(t);
            result.Add(t.ToList());
            path.RemoveAt(path.Count - 1);
            return;
        }
        foreach (var item in graph[v])
        {
            Dfs(graph, item, path);
        }
        path.RemoveAt(path.Count - 1);
    }
}