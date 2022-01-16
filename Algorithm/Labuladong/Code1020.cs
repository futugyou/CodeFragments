namespace Labuladong;

public class Code1020
{
    public static void Exection()
    {
        var grid = new int[4][];
        grid[0] = new int[] { 0, 0, 0, 0 };
        grid[1] = new int[] { 1, 0, 1, 0 };
        grid[2] = new int[] { 0, 1, 1, 0 };
        grid[3] = new int[] { 0, 0, 0, 0 };
        int result = NumEnclaves(grid);
        Console.WriteLine(result);
    }

    private static int NumEnclaves(int[][] grid)
    {
        var m = grid.Length;
        var n = grid[0].Length;
        for (int i = 0; i < m; i++)
        {
            Dfs(grid, i, 0);
            Dfs(grid, i, n - 1);
        }
        for (int i = 0; i < n; i++)
        {
            Dfs(grid, 0, i);
            Dfs(grid, m - 1, i);
        }
        var res = 0;
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (grid[i][j] == 1)
                {
                    res += 1;
                }
            }
        }
        return res;
    }

    private static void Dfs(int[][] grid, int i, int j)
    {
        var m = grid.Length;
        var n = grid[0].Length;
        if (i >= m || i < 0 || j >= n || j < 0 || grid[i][j] == 0)
        {
            return;
        }
        grid[i][j] = 0;
        Dfs(grid, i - 1, j);
        Dfs(grid, i + 1, j);
        Dfs(grid, i, j - 1);
        Dfs(grid, i, j + 1);
    }
}
