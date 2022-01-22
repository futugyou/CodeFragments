namespace Labuladong;

public class Code0174
{
    public static void Exection()
    {
        var grid = new int[3][];
        grid[0] = new int[] { -2, -3, 3 };
        grid[1] = new int[] { -5, -10, 1 };
        grid[2] = new int[] { 10, 30, -5 };
        var result = CalculateMinimumHP(grid);
        Console.WriteLine(result);
    }
    public static int CalculateMinimumHP(int[][] grid)
    {
        var m = grid.Length;
        var n = grid[0].Length;
        var dp = new int[m, n];
        dp[m - 1, n - 1] = grid[m - 1][n - 1] >= 0 ? 1 : (1 - grid[m - 1][n - 1]);
        for (int i = m - 2; i >= 0; i--)
        {
            dp[i, n - 1] = dp[i + 1, n - 1] - grid[i][n - 1];
            if (dp[i, n - 1] <= 0)
            {
                dp[i, n - 1] = 1;
            }
        }
        for (int i = n - 2; i >= 0; i--)
        {
            dp[m - 1, i] = dp[m - 1, i + 1] - grid[m - 1][i];
            if (dp[m - 1, i] <= 0)
            {
                dp[m - 1, i] = 1;
            }
        }
        for (int i = m - 2; i >= 0; i--)
        {
            for (int j = n - 2; j >= 0; j--)
            {
                dp[i, j] = Math.Min(dp[i + 1, j], dp[i, j + 1]) - grid[i][j];
                if (dp[i, j] <= 0)
                {
                    dp[i, j] = 1;
                }
            }
        }
        return dp[0, 0];
    }
}