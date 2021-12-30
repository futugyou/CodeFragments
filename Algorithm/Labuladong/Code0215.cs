namespace Labuladong;

public class Code0215
{

    public static void Exection()
    {
        var nums = new int[] { 3, 2, 3, 1, 2, 4, 5, 5, 6 };
        var k = 4;
        var pq = new PriorityQueue<int, int>();
        foreach (var n in nums)
        {
            pq.Enqueue(n, n);
            if (pq.Count > k)
            {
                pq.Dequeue();
            }
        }
        Console.WriteLine(pq.Peek());
    }
}