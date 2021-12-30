namespace Labuladong;

public class MedianFinder
{
    private class CustomComparer : IComparer<int>
    {
        public int Compare(int x, int y) => (x - y) switch
        {
            > 0 => -1,
            < 0 => 1,
            _ => 0
        };
    }

    private PriorityQueue<int, int> big = new PriorityQueue<int, int>();
    private PriorityQueue<int, int> small = new PriorityQueue<int, int>(new CustomComparer());
    public double FindMedian()
    {
        if (big.Count > small.Count)
        {
            return big.Peek();
        }
        if (big.Count < small.Count)
        {
            return small.Peek();
        }
        return (big.Peek() + small.Peek()) * 1.0 / 2;
    }

    public void AddNum(int num)
    {
        if (big.Count >= small.Count)
        {
            big.Enqueue(num, num);
            var t = big.Dequeue();
            small.Enqueue(t, t);
        }
        else
        {
            small.Enqueue(num, num);
            var t = small.Dequeue();
            big.Enqueue(t, t);
        }
    }
}