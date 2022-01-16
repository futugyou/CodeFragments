using System.Text;

namespace Labuladong;
public class Code0773
{
    public static void Exection()
    {
        var board = new int[2][];
        board[0] = new int[] { 1, 2, 3 };
        board[1] = new int[] { 4, 0, 5 };

        int step = SlidingPuzzle(board);
        Console.WriteLine(step);
    }

    private static int SlidingPuzzle(int[][] board)
    {
        var m = 2;
        var n = 3;
        var sb = new StringBuilder();
        var target = "123450";
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                sb.Append(board[i][j]);
            }
        }
        var start = sb.ToString();
        var neighbor = new int[6][];
        neighbor[0] = new int[] { 1, 3 };
        neighbor[1] = new int[] { 0, 4, 2 };
        neighbor[2] = new int[] { 1, 5 };
        neighbor[3] = new int[] { 0, 4 };
        neighbor[4] = new int[] { 3, 1, 5 };
        neighbor[5] = new int[] { 4, 2 };

        Queue<string> q = new Queue<string>();
        var visited = new List<string>();
        q.Enqueue(start);
        visited.Add(start);
        var step = 0;
        while (q.Any())
        {
            int size = q.Count;
            for (int i = 0; i < size; i++)
            {
                var curr = q.Dequeue();
                if (curr == target)
                {
                    return step;
                }
                int index = 0;
                for (; curr[index] != '0'; index++) ;
                foreach (var adj in neighbor[index])
                {
                    string newstring = Change(curr, adj, index);
                    if (!visited.Contains(newstring))
                    {
                        q.Enqueue(newstring);
                        visited.Add(newstring);
                    }
                }
            }
        }
        return -1;
    }

    private static string Change(string curr, int adj, int index)
    {
        var t = curr.ToArray();
        var c = t[adj];
        t[adj] = t[index];
        t[index] = c;
        return string.Join("", t);
    }
}
