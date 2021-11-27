namespace Labuladong;
public class Code0003
{
    public static void Exection()
    {
        var s = "abcdabcbb";
        var dic = new Dictionary<char, int>();
        var left = 0;
        var right = 0;
        var maxlenght = 0;
        while (right < s.Length)
        {
            var t = s[right];
            if (!dic.Keys.Contains(t))
            {
                dic.Add(t, 1);
            }
            else
            {
                dic[t] = 1 + dic[t];
            }
            right++;

            while (dic[t] > 1)
            {
                var d = s[left];
                if (dic.Keys.Contains(d))
                {
                    dic[d] = dic[d] - 1;
                }
                left++;
            }
            maxlenght = Math.Max(maxlenght, dic.Count);
        }

        Console.WriteLine(maxlenght);
    }
}