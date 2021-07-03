using System;

namespace DailyCodingProblem
{
    public class SingleLinkedList
    {
        public Node Header;

        public void AddNext(int i)
        {
            var n = new Node(i);
            if (Header == null)
            {
                Header = n;
                return;
            }
            var curr = Header;
            while (curr.Next != null)
            {
                curr = curr.Next;
            }
            curr.Next = n;
        }

        public void ConstNode()
        {
            var nums = new int[] { 9, 8, 0, 3, 6, 1, 4, 7, 5 };
            for (int i = 0; i < nums.Length; i++)
            {
                AddNext(nums[i]);
            }
            Display();
        }

        public void Reverse()
        {
            if (Header == null)
            {
                return;
            }
            Node prev = null;
            var curr = Header;
            while (curr != null)
            {
                var next = curr.Next;
                curr.Next = prev;
                prev = curr;
                curr = next;
            }
            Header = prev; 
            Display();
        }

        public void Display()
        {
            var next = Header;
            while (next != null)
            {
                Console.Write(" " + next.Value);
                next = next.Next;
            }
            Console.WriteLine();
        }
    }

    public class Node
    {
        public int Value { get; set; }
        public Node Next { get; set; }
        public Node(int value)
        {
            Value = value;
        }
    }
}
