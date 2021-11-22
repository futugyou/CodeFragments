namespace Labuladong;
public class Code0392
{
    public static void Exection()
    {
        var s = "abc";
        var t = "ahbgdc";
        var i = 0;
        var j = 0;
        while (i < s.Length && j < t.Length)
        {
            if (s[i] == t[j])
            {
                i++;
            }
            j++;
        }
        Console.WriteLine(i == s.Length);
    }
}