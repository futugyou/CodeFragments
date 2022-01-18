namespace Labuladong;
public class Code0011
{
    public static void Exection()
    {
        var s = "aa";
        var p = "a";
        bool res = IsMatch(s, p);
        Console.WriteLine(res);
    }

    private static bool IsMatch(string s, string p)
    {
        return Dp(s, 0, p, 0);
    }

    private static Dictionary<string, bool> memo = new();
    private static bool Dp(string s, int i, string p, int j)
    {
        var m = s.Length;
        var n = p.Length;
        if (j == n)
        {
            return i == m;
        }
        if (i == m)
        {
            if ((n - j) % 2 == 1)
            {
                return false;
            }
            for (; j + 1 < n; j += 2)
            {
                if (p[j + 1] != '*')
                {
                    return false;
                }
            }
            return true;
        }

        string key = i + "," + j;
        if (memo.ContainsKey(key))
        {
            return memo[key];
        }
        bool res = false;

        if (s[i] == p[j] || p[j] == '.')
        {
            if (j < n - 1 && p[j + 1] == '*')
            {
                res = Dp(s, i, p, j + 2) || Dp(s, i + 1, p, j);
            }
            else
            {
                res = Dp(s, i + 1, p, j + 1);
            }
        }
        else
        {
            if (j < n - 1 && p[j + 1] == '*')
            {
                res = Dp(s, i, p, j + 2);
            }
            else
            {
                res = false;
            }
        }
        memo.Add(key, res);
        return res;
    }
}