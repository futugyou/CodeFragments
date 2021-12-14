namespace Labuladong;

public class Code0025
{
    public static void Exection()
    {
        var root = new LinkNode();
        ReverseKGroup(root, 3);
    }

    public static LinkNode ReverseKGroup(LinkNode root, int k)
    {
        if (root == null)
        {
            return null;
        }
        var a = root;
        var b = root;
        for (int i = 0; i < k; i++)
        {
            if (b == null)
            {
                return root;
            }
            b = b.Next;
        }
        var newHead = Reverse(a, b);
        a.Next = ReverseKGroup(b, k);
        return newHead;
    }

    private static LinkNode Reverse(LinkNode a, LinkNode b)
    {
        LinkNode pre = null;
        LinkNode curr = a;
        LinkNode next = a;
        while (curr != b)
        {
            next = curr.Next;
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