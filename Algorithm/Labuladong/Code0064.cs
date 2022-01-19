namespace Labuladong;

public class Code0064
{
    public static void Exection()
    {
        var grid = new int[3][] { new int[] { 1, 3, 1 }, new int[] { 1, 5, 1 }, new int[] { 4, 2, 1 } };
        int res = MinPathSum(grid);
        Console.WriteLine(res);
    }

    private static int MinPathSum(int[][] grid)
    {
        var m = grid.Length;
        var n = grid[0].Length;
        var res = new int[m, n];
        res[0, 0] = grid[0][0];
        for (int i = 1; i < m; i++)
        {
            res[i, 0] = res[i - 1, 0] + grid[i][0];
        }
        for (int i = 1; i < n; i++)
        {
            res[0, i] = res[0, i - 1] + grid[0][i];
        }
        for (int i = 1; i < m; i++)
        {
            for (int j = 1; j < n; j++)
            {
                res[i, j] = Math.Min(res[i - 1, j], res[i, j - 1]) + grid[i][j];
            }
        }
        return res[m - 1, n - 1];
    }
}