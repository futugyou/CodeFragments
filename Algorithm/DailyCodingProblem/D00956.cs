using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given k sorted singly linked lists, write a function to merge all the lists into one sorted singly linked list.
    /// </summary>
    public class D00956
    {
        private static Node head;
        private static Node curr;
        public static void Exection()
        {
            var nodes = new Node[2];
            var n0 = new Node(0);
            var n1 = new Node(1);
            var n2 = new Node(2);
            var n3 = new Node(3);
            var n4 = new Node(4);
            var n5 = new Node(5);
            n0.Next = n1;
            n2.Next = n3;
            n3.Next = n4;
            n4.Next = n5;
            nodes[0] = n0;
            nodes[1] = n2;
            var min = new int[2];
            min[0] = nodes[0].Value;
            min[1] = nodes[1].Value;

            Merge(nodes, min);
            while (head != null)
            {
                Console.Write(head.Value + " ");
                head = head.Next;
            }
        }

        private static void Merge(Node[] nodes, int[] min)
        {
            bool flag = false;
            foreach (var item in nodes)
            {
                if (item != null)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag) return;

            (int index, int value) = MinInArray(min);
            if (curr == null)
            {
                curr = new Node(value);
                if (head == null)
                {
                    head = curr;
                }
            }
            else
            {
                curr.Next = new Node(value);
                curr = curr.Next;
            }
            var node = nodes[index];
            if (node != null)
            {
                node = node.Next;
                nodes[index] = node;
                min[index] = node != null ? node.Value : int.MaxValue;
            }
            Merge(nodes, min);
        }

        private static (int index, int value) MinInArray(int[] min)
        {
            int result = int.MaxValue;
            int index = min.Length;
            for (int i = 0; i < min.Length; i++)
            {
                if (min[i] < result)
                {
                    result = min[i];
                    index = i;
                }
            }
            return (index, result);
        }

        private class Node
        {
            public int Value { get; set; }
            public Node Next { get; set; }
            public Node(int value)
            {
                Value = value;
            }
        }
    }
}
