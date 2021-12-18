namespace Labuladong;

public class Code0234
{
    public static void Exection()
    {

    }

    public static bool IsPalindrome(LinkNode head)
    {
        LinkNode fast = head;
        LinkNode slow = head;
        while (fast != null && fast.Next != null)
        {
            fast = fast.Next.Next;
            slow = slow.Next;
        }
        if (fast != null)
        {
            slow = slow.Next;
        }
        LinkNode left = head;
        LinkNode right = Reverse(slow);
        while (right != null)
        {
            if (right.Value != left.Value)
            {
                return false;
            }
            right = right.Next;
            left = left.Next;
        }
        return true;
    }

    private static LinkNode Reverse(LinkNode head)
    {
        LinkNode pre = null;
        LinkNode curr = head;
        while (curr != null)
        {
            var next = curr.Next;
            curr.Next = pre;
            pre = curr;
            curr = next;
        }
        return pre;
    }

    public class LinkNode
    {
        public int Value { get; set; }
        public LinkNode Next { get; set; }
    }
}