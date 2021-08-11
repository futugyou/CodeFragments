using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given two singly linked lists that intersect at some point, find the intersecting node. 
    /// The lists are non-cyclical.
    /// For example, given A = 3 -> 7 -> 8 -> 10 and B = 99-> 1 -> 8 -> 10, return the node with value 8.
    /// In this example, assume nodes with the same value are the exact same node objects.
    /// Do this in O(M + N) time (where M and N are the lengths of the lists) and "constant space".
    /// </summary>
    public class D00963
    {
        public static void Exection()
        {
            var a = new Node { Value = 1, Next = new Node { Value = 2, Next = new Node { Value = 7, Next = new Node { Value = 8, Next = new Node { Value = 10 } } } } };
            var b = new Node { Value = 0, Next = new Node { Value = 5, Next = new Node { Value = 8, Next = new Node { Value = 10 } } } };
            // How can be "constant space"
            var c = a;
            var d = b;
            while (a != null && b != null)
            {
                if (a.Value == b.Value)
                {
                    Console.WriteLine(a.Value);
                    return;
                }
                a = a.Next;
                b = b.Next;
                if (a == null)
                {
                    a = d;
                }
                if (b == null)
                {
                    b = c;
                }
            }
        }

        class Node
        {
            public int Value { get; set; }
            public Node Next { get; set; }
        }
    }
}
