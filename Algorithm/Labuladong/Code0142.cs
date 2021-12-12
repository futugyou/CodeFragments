namespace Labuladong;

public class Code0142
{
    public static void Exection()
    {
        var root = new LinkNode();
        GetCycle(root);
    }

    public static LinkNode GetCycle(LinkNode root)
    {
        var res = false;
        var fast = root;
        var slow = root;
        while (fast != null && fast.Next != null)
        {
             fast = fast.Next.Next;
             slow = slow.Next;
             if (fast == slow)
             {
                 break;
             }
        }
        if (fast == null || fast.Next == null)
        {
            return null;
        }
        fast = root;
        while (fast != slow)
        {
              fast = fast.Next;
              slow = slow.Next;
        }
        return fast;
    }

    public class LinkNode
    {
        public int Value { get; set; }
        public LinkNode Next { get; set; }
    }
}