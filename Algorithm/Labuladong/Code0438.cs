namespace Labuladong;
public class Code0438
{
    public static void Exection()
    {
        var s = "abcababa";
        var t = "ab";
        var need = new Dictionary<char, int>();
        var windows = new Dictionary<char, int>();
        foreach (var item in t)
        {
            if (need.ContainsKey(item))
            {
                need[item]++;
            }
            else
            {
                need.Add(item, 1);
            }
        }
        var result = new List<int>();
        var left = 0;
        var right = 0;
        var valid = 0;
        while (right < s.Length)
        {
            var c = s[right];
            right++;

            if (need.ContainsKey(c))
            {
                if (windows.ContainsKey(c))
                {
                    windows[c]++;
                }
                else
                {
                    windows.Add(c, 1);
                }
                if (need[c] == windows[c])
                {
                    valid++;
                }
            }

            while (right - left >= t.Length)
            {
                if (valid == need.Count)
                {
                    result.Add(left);
                }

                var d = s[left];
                left++;
                if (need.ContainsKey(d))
                {
                    if (windows.ContainsKey(d))
                    {
                        if (windows[d] == need[d])
                        {
                            valid--;
                        }
                        windows[d]--;
                    }
                }
            }
        }
        Console.WriteLine(string.Join(",", result));
    }
}