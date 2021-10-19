using System.Text;
namespace Labuladong;
public class Code0694
{
    public static void Exection()
    {
        int[,] grid =  {
            {1, 1, 0, 1, 0 },
            {1, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {1, 1, 0, 1, 1 },
            };

        int m = grid.GetLength(0);
        int n = grid.GetLength(1);
        Console.WriteLine(m + " " + n);
        HashSet<string> islands = new HashSet<string>();
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (grid[i, j] == 1)
                {
                    Console.WriteLine("  " + i + " " + j);
                    StringBuilder sb = new StringBuilder();
                    Dfs(grid, i, j, sb, 0);
                    islands.Add(sb.ToString());
                }
            }
        }
        foreach (var item in islands)
        {
            Console.WriteLine(item);
        }
    }

    public static void Dfs(int[,] grid, int i, int j, StringBuilder sb, int dir)
    {
        int m = grid.GetLength(0);
        int n = grid.GetLength(1);
        if (i < 0 || j < 0 || i >= m || j >= n)
        {
            return;
        }
        if (grid[i, j] == 0)
        {
            return;
        }
        grid[i, j] = 0;
        sb.Append(dir).Append(',');
        Dfs(grid, i + 1, j, sb, 1);
        Dfs(grid, i, j + 1, sb, 2);
        Dfs(grid, i - 1, j, sb, 3);
        Dfs(grid, i, j - 1, sb, 4);
        sb.Append(-dir).Append(',');
    }
}