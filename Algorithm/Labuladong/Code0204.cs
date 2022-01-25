namespace Labuladong;

public class Code0204
{
    public static void Exection()
    {
        var result = CountPrimes(25);
        Console.WriteLine(result);
    }

    private static int CountPrimes(int n)
    {
        var dp = new bool[n];
        Array.Fill(dp, true);
        for (int i = 2; i * i < n; i++)
        {
            if (dp[i])
            {
                for (int j = i * i; j < n; j = j + i)
                {
                    dp[j] = false;
                }
            }
        }
        var res = 0;
        for (int i = 2; i < n; i++)
        {
            if (dp[i])
            {
                res++;
            }
        }
        return res;
    }
}
