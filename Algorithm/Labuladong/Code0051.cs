namespace Labuladong;

public class Code0051
{
    public static void Exection()
    {
        NQueue(4);
        foreach (var item in result)
        {
            foreach (var i in item)
            {
                Console.WriteLine(string.Join("|", i));
            }
            Console.WriteLine("=================");
        }
    }

    private static List<List<string[]>> result = new();

    private static List<List<string[]>> NQueue(int n)
    {
        var res = new List<string[]>(n);
        for (int i = 0; i < n; i++)
        {
            var t = (string[])Array.CreateInstance(typeof(string), n);
            Array.Fill(t, ".");
            res.Add(t);
        }
        BackTrack(res, 0);
        return result;
    }

    private static void BackTrack(List<string[]> res, int row)
    {
        var n = res.Count;
        if (row == n)
        {
            var copy = new List<string[]>(n);
            foreach (var item in res)
            {
                var t = new string[n];
                Array.Copy(item, t, n);
                copy.Add(t);
            }
            result.Add(copy);
            return;
        }

        for (int col = 0; col < n; col++)
        {
            if (IsValid(res, row, col))
            {
                res[row][col] = "Q";
                BackTrack(res, row + 1);
                res[row][col] = ".";
            }
        }
    }

    private static bool IsValid(List<string[]> res, int row, int col)
    {
        var n = res.Count;
        for (int i = 0; i < n; i++)
        {
            if (res[i][col] == "Q")
            {
                return false;
            }
        }
        for (int i = row - 1, j = col + 1; i >= 0 && j < n; i--, j++)
        {
            if (res[i][j] == "Q")
            {
                return false;
            }
        }
        for (int i = row - 1, j = col - 1; i >= 0 && j >= 0; i--, j--)
        {
            if (res[i][j] == "Q")
            {
                return false;
            }
        }
        return true;
    }
}