namespace Labuladong;

public class Code0092
{
    public static void Exection()
    {

    }

    public static ListNode ReverseBetween(ListNode head, int m, int n)
    {
        if (m == 1)
        {
            return ReverseN(head, n);
        }
        head.Next = ReverseBetween(head.Next, n - 1, m - 1);
        return head;
    }

    private static ListNode Next;
    public static ListNode ReverseN(ListNode head, int n)
    {
        if (n == 1)
        {
            Next = head.Next;
            return head;
        }
        var last = ReverseN(head.Next, n - 1);
        head.Next.Next = head;
        head.Next = Next;
        return last;
    }

    public class ListNode
    {
        public int Value { get; set; }
        public ListNode Next { get; set; }
    }
}