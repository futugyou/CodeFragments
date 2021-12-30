namespace Labuladong;

public class MonotonicQueue
{
    private LinkedList<int> q = new LinkedList<int>();
    public void Push(int n)
    {
        while (q.Any() && q.Last() < n)
        {
            q.RemoveLast();
        }
        q.AddLast(n);
    }

    public int Max()
    {
        return q.First();
    }

    public void Pop(int n)
    {
        if (n == q.First())
        {
            q.RemoveFirst();
        }
    }
}