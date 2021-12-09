namespace Labuladong;

public class Code0023
{

    public static void Exection()
    {

    }

    public static ListNode MergeKLists(ListNode[] lists)
    {
        var dummy = new ListNode() { Value = -1 };
        var p = dummy;
        var qp = new PriorityQueue<ListNode, int>();
        foreach (var item in lists)
        {
            qp.Enqueue(item, item.Value);
        }

        while (qp.Count > 0)
        {
            var node = qp.Dequeue();
            p.Next = node;
            if (node.Next != null)
            {
                qp.Enqueue(node.Next, node.Next.Value);
            }
            p = p.Next;
        }
        return dummy.Next;
    }

    public class ListNode
    {
        public int Value { get; set; }
        public ListNode Next { get; set; }
    }
}