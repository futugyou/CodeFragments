namespace Labuladong;
public class Code0076
{
    public static void Exection()
    {
        var s = "qwertyuio";
        var t = "rtuy";
        var need = new Dictionary<char, int>();
        foreach (var item in t)
        {
            need.Add(item, 1);
        }
        var windows = new Dictionary<char, int>();
        var left = 0;
        var right = 0;
        var valid = 0;
        var start = 0;
        var len = int.MaxValue;
        while (right < s.Length)
        {
            var c = s[right];
            right++;
            if (need.ContainsKey(c))
            {
                if (windows.ContainsKey(c))
                {
                    windows[c] = 1 + windows[c];
                }
                else
                {
                    windows[c] = 1;
                }
                if (windows[c] == need[c])
                {
                    valid++;
                }
            }

            while (valid == need.Count)
            {
                if (right - left < len)
                {
                    len = right - left;
                    start = left;
                }
                var d = s[left];
                left++;
                if (need.ContainsKey(d))
                {
                    if (windows[d] == need[d])
                    {
                        valid--;
                    }
                    windows[d]--;
                }
            }
        }
        if (len != int.MaxValue)
        {
            Console.WriteLine(s.Substring(start, len));
        }
        else
        {
            Console.WriteLine("null");
        }
    }
}