namespace Labuladong;

public class Code0876
{
    public static void Exection()
    {
        var root = new LinkNode();
        GetMid(root);
    }

    public static LinkNode GetMid(LinkNode root)
    {
        var fast = root;
        var slow = root;
        while (fast != null && fast.Next != null)
        {
            fast = fast.Next.Next;
            slow = slow.Next;
        }
        return slow;
    }

    public class LinkNode
    {
        public int Value { get; set; }
        public LinkNode Next { get; set; }
    }
}