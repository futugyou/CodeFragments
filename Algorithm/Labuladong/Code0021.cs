namespace Labuladong;

public class Code0021
{

    public static void Exection()
    {
        var dummy = new LinkList(){Value=-1};
        var p = dummy;
        var p1 = new LinkList();
        var p2 = new LinkList();
        while (p1!=nukk&&p2!=null)
        {
             if (p1.Value>p2.Value)
             {
                 p.Next = p2;
                 p2 = p2.Next;
             }
             else
             {
                 p.Next = p1;
                 p1 = p1.Next;
             }
             p=p.Next;
        }
        if (p1!=null)
        {
            p.Next=p1;
        }
        if (p2!=null)
        {
            p.Next=p2;
        }
        return dummy.Next;
    }
    
    public class LinkList
    {
        public int Value { get; set; }
        public LinkList Next { get; set; } 
    }
}