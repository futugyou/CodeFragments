using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a linked list and a positive integer k, rotate the list to the right by k places.
    /// For example, given the linked list 7 -> 7 -> 3 -> 5 and k = 2, it should become 3 -> 5 -> 7 -> 7.
    /// Given the linked list 1 -> 2 -> 3 -> 4 -> 5 and k = 3, it should become 3 -> 4 -> 5 -> 1 -> 2.
    /// </summary>
    public class D00927
    {
        private class LinkedList
        {
            public int Value { get; set; }
            public LinkedList Next { get; set; }
        }
        public static void Exection()
        {
            //var linked = new LinkedList
            //{
            //    Value = 7,
            //    Next = new LinkedList
            //    {
            //        Value = 8,
            //        Next = new LinkedList
            //        {
            //            Value = 3,
            //            Next = new LinkedList
            //            {
            //                Value = 5
            //            }
            //        }
            //    }
            //};

            var linked = new LinkedList
            {
                Value = 1,
                Next = new LinkedList
                {
                    Value = 2,
                    Next = new LinkedList
                    {
                        Value = 3,
                        Next = new LinkedList
                        {
                            Value = 4,
                            Next = new LinkedList
                            {
                                Value = 5
                            }
                        }
                    }
                }
            };
            Display(linked);
            int k = 0;
            var index = 0;
            LinkedList first = linked;
            LinkedList second = linked;
            while (index < k && first != null)
            {
                first = first.Next;
                index++;
            }
            if (index != k || index <= 0 || first == null)
            {
                Display(linked);
                return;
            }
            while (first != null)
            {
                first = first.Next;
                second = second.Next;
            }
            first = linked;
            while (first.Next != second)
            {
                first = first.Next;
            }
            first.Next = null;
            var firth = second;
            while (firth.Next != null)
            {
                firth = firth.Next;
            }
            firth.Next = linked;
            Display(second);
        }

        private static void Display(LinkedList second)
        {
            var t = second;
            while (t != null)
            {
                Console.Write(t.Value + " ");
                t = t.Next;
            }
            Console.WriteLine();
        }
    }
}
