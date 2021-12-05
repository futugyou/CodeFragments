namespace Labuladong;
public class Code0019
{
    public static void Exection()
    {
        var node = new ListNode { Value = 1, Next = new ListNode { Value = 2, Next = new ListNode { Value = 3, Next = new ListNode { Value = 4 } } } };
        var last = 2;
        ListNode head = RamoveLastIndex(node, last);
    }

    private static ListNode RamoveLastIndex(ListNode node, int last)
    {
        var dammy = new ListNode { Value = -1, Next = node };
        var found = FindLastByIndex(dammy, last + 1);
        if (found != null && found.Next != null)
        {
            found.Next = found.Next.Next;
        }
        return dammy.Next;
    }

    private static ListNode FindLastByIndex(ListNode head, int v)
    {
        var fast = head;
        for (int i = 0; i < v; i++)
        {
            fast = fast.Next;
        }
        var slow = head;
        while (fast != null)
        {
            fast = fast.Next;
            slow = slow.Next;
        }
        return slow;
    }

    public class ListNode
    {
        public int Value { get; set; }
        public ListNode Next { get; set; }
    }
}