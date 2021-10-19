namespace Labuladong;
public class Code1254
{
    public static void Exection()
    {
        int[,] grid =  {
            {1, 1, 0, 1, 0 },
            {1, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {1, 1, 0, 1, 1 },
            };

        int res = 0;
        int m = grid.GetLength(0);
        int n = grid.GetLength(1);
        Console.WriteLine(m + " " + n);

        for (int j = 0; j < n; j++)
        {
            // 把靠上边的岛屿淹掉
            Dfs(grid, 0, j);
            // 把靠下边的岛屿淹掉
            Dfs(grid, m - 1, j);
        }
        for (int i = 0; i < m; i++)
        {
            // 把靠左边的岛屿淹掉
            Dfs(grid, i, 0);
            // 把靠右边的岛屿淹掉
            Dfs(grid, i, n - 1);
        }

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (grid[i, j] == 1)
                {
                    res++;
                    Console.WriteLine("  " + i + " " + j);
                    Dfs(grid, i, j);
                }
            }
        }
        Console.WriteLine(res);
    }

    public static void Dfs(int[,] grid, int i, int j)
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
        Dfs(grid, i + 1, j);
        Dfs(grid, i, j + 1);
        Dfs(grid, i - 1, j);
        Dfs(grid, i, j - 1);
    }
}