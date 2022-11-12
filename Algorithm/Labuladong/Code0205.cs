namespace Labuladong;

public class Code0205
{
    public static void Exection()
    {
        var result = IsIsomorphic("egg","add");
        Console.WriteLine(result);
    }

    private static  bool IsIsomorphic(string s, string t)
    {
        var dic = new Dictionary<char, int>();
        for (int i = 0; i < s.Length; i++)
        {
            if (dic.ContainsKey(s[i]))
            {
                var c = dic[s[i]];
                if (c != s[i] - t[i])
                {
                    return false;
                }
            }
            else
            {
                dic.Add(s[i], s[i] - t[i]);
            }
        }

        dic.Clear();

        for (int i = 0; i < s.Length; i++)
        {
            if (dic.ContainsKey(t[i]))
            {
                var c = dic[t[i]];
                if (c != t[i] - s[i])
                {
                    return false;
                }
            }
            else
            {
                dic.Add(t[i], t[i] - s[i]);
            }
        }

        return true;

    }
}
