namespace Labuladong;
public class Code0870
{
    public static void Exection()
    {
        var a = new int[] { 2, 7, 11, 15 };
        var b = new int[] { 1, 10, 4, 11 };
        var res = new int[a.Length];
        var pq = new PriorityQueue<int[], int>(new IntComparer());
        for (int i = 0; i < b.Length; i++)
        {
            pq.Enqueue(new int[] { i, b[i] }, b[i]);
        }

        Array.Sort(a);
        var left = 0;
        var right = a.Length - 1;
        while (pq.Count > 0)
        {
            var t = pq.Dequeue();
            if (a[right] > t[1])
            {
                res[t[0]] = a[right];
                right--;
            }
            else
            {
                res[t[0]] = a[left];
                left++;
            }
        }
        Console.WriteLine(string.Join(",", res));
    }

    private class IntComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x == y)
            {
                return 0;
            }
            if (x > y)
            {
                return -1;
            }
            return 1;
        }
    }
}