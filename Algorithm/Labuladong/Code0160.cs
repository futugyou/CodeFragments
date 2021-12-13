namespace Labuladong;

public class Code0160
{
    public static void Exection()
    {
        var roota = new LinkNode();
        var rootb = new LinkNode();
        GetJion(roota,rootb);
    }

    public static LinkNode GetJion(LinkNode roota,LinkNode rootb)
    {
        var heada = roota;
        var headv = rootb;
        while (heada != headv)
        {
             if (heada == null)
             {
                 heada = rootb;                 
             }
             else
             {
                 heada= heada.Next;
             }
             if (headv == null)
             {
                 headv = roota;                 
             }
             else
             {
                 headv= headv.Next;
             }
        } 
        return heada;
    }

    public class LinkNode
    {
        public int Value { get; set; }
        public LinkNode Next { get; set; }
    }
}