namespace Labuladong;
public class Code0567
{
    public static void Exection()
    {
        var s1 = "ab";
        var s2 = "eidbaooo";
        var need = new Dictionary<char, int>();
        foreach (var item in s1)
        {
            if (need.ContainsKey(item))
            {
                need[item] += 1;
            }
            else
            {
                need.Add(item, 1);
            }
        }
        var windows = new Dictionary<char, int>();

        var left = 0;
        var right = 0;
        var valid = 0;
        var have = false;
        while (right < s2.Length)
        {
            var c = s2[right];
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
                if (windows[c] == need[c])
                {
                    valid++;
                }
            }

            while (right - left >= s1.Length)
            {
                if (need.Count == valid)
                {
                    have = true;
                    break;
                }
                var d = s2[left];
                left++;

                if (need.ContainsKey(d))
                {
                    if (windows.ContainsKey(d))
                    {
                        windows[d]--;
                        if (windows[d] == need[d])
                        {
                            valid--;
                        }
                    }
                }
            }
            if (have)
            {
                break;
            }
        }
        Console.WriteLine(have);
    }
}