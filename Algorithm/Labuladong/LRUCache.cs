namespace Labuladong;

public class LRUCache
{
    public class Node
    {
        public int Key { get; set; }
        public int Value { get; set; }
        public Node Next { get; set; }
        public Node Prev { get; set; }
        public Node(int key, int value)
        {
            Key = key;
            Value = value;
        }
    }

    public class DoubleLinkList
    {
        private Node head = new Node(0, 0);
        private Node tail = new Node(0, 0);
        private int size = 0;
        public DoubleLinkList()
        {
            head.Next = tail;
            tail.Prev = head;
        }

        public void AddLast(Node x)
        {
            x.Prev = tail.Prev;
            x.Next = tail;
            tail.Prev.Next = x;
            tail.Prev = x;
            size++;
        }

        public void Remove(Node x)
        {
            x.Prev.Next = x.Next;
            x.Next.Prev = x.Prev;
            size--;
        }

        public Node? RemoveFirst()
        {
            if (head.Next == tail)
            {
                return null;
            }
            var f = head.Next;
            Remove(f);
            return f;
        }
        public int Size => size;
    }
    private Dictionary<int, Node> dic = new Dictionary<int, Node>();
    private DoubleLinkList list = new DoubleLinkList();
    private int cap = 0;
    public LRUCache(int cap)
    {
        this.cap = cap;
    }

    public int Get(int key)
    {
        if (dic.ContainsKey(key))
        {
            MakeRecently(key);
            return dic[key].Value;
        }
        return -1;
    }

    public void Put(int key, int value)
    {
        if (dic.ContainsKey(key))
        {
            DeleteKey(key);
            AddRecently(key, value);
            return;
        }
        if (cap == list.Size)
        {
            RemoveLeastRecently();
        }
        AddRecently(key, value);
    }

    private void MakeRecently(int key)
    {
        var x = dic[key];
        list.Remove(x);
        list.AddLast(x);
    }

    private void AddRecently(int key, int value)
    {
        var x = new Node(key, value);
        list.AddLast(x);
        dic.Add(key, x);
    }

    private void DeleteKey(int key)
    {
        var x = dic[key];
        list.Remove(x);
        dic.Remove(key);
    }

    private void RemoveLeastRecently()
    {
        var d = list.RemoveFirst();
        if (d != null)
        {
            dic.Remove(d.Key);
        }
    }
}