namespace Labuladong;
public class Code0232
{
    public static void Exection()
    {

    }

    public class MyQueue
    {
        private int currValue;
        private Stack<int> stack;

        public MyQueue()
        {
            currValue = -1;
            stack = new Stack<int>();
        }

        public void Push(int x)
        {
            if (!stack.Any())
            {
                currValue = x;
            }
            stack.Push(x);
        }

        public int Pop()
        {
            var count = stack.Count;
            if (count == 0)
            {
                return -1;
            }
            if (count == 1)
            {
                currValue = -1;
                return stack.Pop();
            }
            var tmp = new Stack<int>();
            while (count > 1)
            {
                tmp.Push(stack.Pop());
                count--;
            }
            currValue = tmp.Peek();
            var res = stack.Pop();
            while (tmp.Any())
            {
                stack.Push(tmp.Pop());
            }
            return res;
        }

        public int Peek()
        {
            return currValue;
        }

        public bool IsEmpty()
        {
            return !stack.Any();
        }
    }

    public class MyQueue2
    {
        private Stack<int> stack;
        private Stack<int> stack1;

        public MyQueue2()
        {
            stack = new Stack<int>();
            stack1 = new Stack<int>();
        }

        public void Push(int x)
        {
            stack.Push(x);
        }

        public int Pop()
        {
            Peek();
            return stack1.Pop();
        }

        public int Peek()
        {
            if (!stack1.Any())
            {
                while (stack.Any())
                {
                    stack1.Push(stack.Pop());
                }
            }
            return stack1.Peek();
        }

        public bool IsEmpty()
        {
            return !stack.Any();
        }
    }
}
