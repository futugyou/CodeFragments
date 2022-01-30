namespace Labuladong;
public class Code0931
{
    public static void Exection()
    {
        var matrix = new int[3][]{
            new int[]{ 2, 1, 3 },
            new int[]{ 6, 5, 4 },
            new int[]{ 7, 8, 9 },

        };
        var result = FallingPath(matrix);
        Console.WriteLine(result);
    }

    private static int FallingPath(int[][] matrix)
    {
        var n = matrix.Length;
        var dp = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == 0)
                {
                    dp[i, j] = matrix[i][j];
                }
                else
                {
                    dp[i, j] = int.MaxValue;
                }
            }
        }

        var res = int.MaxValue;
        for (int i = 1; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (j == 0)
                {
                    dp[i, j] = matrix[i][j] + Math.Min(dp[i - 1, j], dp[i - 1, j + 1]);
                }
                else if (j == n - 1)
                {
                    dp[i, j] = matrix[i][j] + Math.Min(dp[i - 1, j], dp[i - 1, j - 1]);
                }
                else
                {
                    dp[i, j] = matrix[i][j] + Math.Min(dp[i - 1, j], Math.Min(dp[i - 1, j + 1], dp[i - 1, j - 1]));
                }
            }
        }
        for (int i = 0; i < n; i++)
        {
            res = Math.Min(res, dp[n - 1, i]);
        }
        return res;
    }
}
