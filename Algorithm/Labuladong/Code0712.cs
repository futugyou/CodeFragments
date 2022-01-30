namespace Labuladong;
public class Code0712
{
    public static void Exection()
    {
        var str1 = "sea";
        var str2 = "eat";
        var result = CountChange(str1, str2);
        Console.WriteLine(result);
    }


    private static int[,] memo;
    private static int CountChange(string str1, string str2)
    {
        var m = str1.Length;
        var n = str2.Length;
        memo = new int[m, n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                memo[i, j] = -1;
            }
        }
        return Dp(str1, 0, str2, 0);
    }

    private static int Dp(string str1, int i, string str2, int j)
    {
        var res = 0;
        if (i == str1.Length)
        {
            for (; j < str2.Length; j++)
            {

                res += (int)str2[j];
            }
            return res;
        }
        if (j == str2.Length)
        {
            for (; i < str1.Length; i++)
            {

                res += (int)str1[i];
            }
            return res;
        }

        if ((memo[i, j] != -1))
        {
            return memo[i, j];
        }
        if (str1[i] == str2[j])
        {
            res = Dp(str1, i + 1, str2, j + 1);
        }
        else
        {
            res = Math.Min((int)str1[i] + Dp(str1, i + 1, str2, j),
            (int)str2[j] + Dp(str1, i, str2, j + 1)
            );
        }
        memo[i, j] = res;
        return res;
    }
}
