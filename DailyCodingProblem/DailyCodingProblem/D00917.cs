using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a linked list of numbers and a pivot k,
    /// partition the linked list so that all nodes less than k come before nodes greater than or equal to k.
    /// For example, given the linked list 5 -> 1 -> 8 -> 0 -> 3 and k = 3, the solution could be 1 -> 0 -> 5 -> 8 -> 3.
    /// </summary>
    public class D00917
    {
        public void Partition(int k)
        {
            var link = new SingleLinkedList();
            link.ConstNode();

            Node left = null;
            Node right = null;
            Node header = link.Header!;
            while (header != null)
            {
                var t = new Node(header.Value);
                if (header.Value >= k)
                {
                    if (right != null)
                    {
                        t.Next = right;
                        right = t;
                    }
                    else
                    {
                        right = t;
                    }
                }
                else
                {
                    if (left != null)
                    {
                        t.Next = left;
                        left = t;
                    }
                    else
                    {
                        left = t;
                    }

                }
                header = header.Next;
            }

            if (left != null)
            {
                var curr = left;
                while (curr.Next != null)
                {
                    curr = curr.Next;
                }
                curr.Next = right;
            }
            link.Header = left;
            link.Display();
        }
    }
}
