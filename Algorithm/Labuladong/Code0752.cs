namespace Labuladong;
public class Code0752
{
    public static void Exection()
    {
        var deadends = new string[] { "0201", "0101", "0102", "1212", "2002" };
        var target = "0202";
        int step = OpenLock(deadends, target);
        Console.WriteLine(step);
    }

    private static int OpenLock(string[] deadends, string target)
    {
        List<string> visited = new();
        Queue<string> q = new();
        q.Enqueue("0000");
        visited.Add("0000");
        int step = 0;
        while (q.Any())
        {
            var size = q.Count;
            for (int i = 0; i < size; i++)
            {
                var curr = q.Dequeue();
                if (deadends.Contains(curr))
                {
                    continue;
                }
                if (curr == target)
                {
                    return step;
                }
                for (int j = 0; j < 4; j++)
                {
                    var up = UpOne(curr, j);
                    if (!deadends.Contains(up))
                    {
                        visited.Add(up);
                        q.Enqueue(up);
                    }
                    var down = DownOne(curr, j);
                    if (!deadends.Contains(down))
                    {
                        visited.Add(down);
                        q.Enqueue(down);
                    }
                }
            }
            step++;
        }
        return -1;
    }

    private static string DownOne(string curr, int j)
    {
        var t = curr.ToArray();
        var c = t[j];
        if (c == '0')
        {
            c = '9';
        }
        else
        {
            c = (char)(c - 1);
        }
        return string.Join("", t);
    }

    private static string UpOne(string curr, int j)
    {
        var t = curr.ToArray();
        var c = t[j];
        if (c == '9')
        {
            c = '0';
        }
        else
        {
            c = (char)(c + 1);
        }
        return string.Join("", t);
    }
}