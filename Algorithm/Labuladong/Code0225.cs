
namespace Labuladong;
public class Code0225
{
    public static void Exection()
    {

    }

    public class MyStack
    {
        private int currValue;
        private Queue<int> q;

        public MyStack()
        {
            currValue = -1;
            q = new Queue<int>();
        }

        public void Push(int x)
        {
            currValue = x;
            q.Enqueue(x);
        }

        public int Pop()
        {
            var count = q.Count;
            if (count == 0)
            {
                return -1;
            }
            while (count > 2)
            {
                q.Enqueue(q.Dequeue());
                count--;
            }
            currValue = q.Peek();
            q.Enqueue(q.Dequeue());
            return q.Dequeue();
        }

        public int Peek()
        {
            return currValue;
        }

        public bool IsEmpty()
        {
            return !q.Any();
        }
    }
}
