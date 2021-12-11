namespace Labuladong;

public class Code0141
{

    public static void Exection()
    {
        var root = new LinkNode();
        var res = false;
        var fast = root;
        var slow = root;
        while (fast!=null&&fast.Next!=null)
        {
             fast = fast.Next.Next;
             slow = slow.Next;
             if (fast == slow)
             {
                 res = true;
                 break;
             }
        }
        Console.WriteLine(res);
    }
    public class LinkNode
    {
        public int Value { get; set; }
        public LinkNode Next { get; set; }
    }
}