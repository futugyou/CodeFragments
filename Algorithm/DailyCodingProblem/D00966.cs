namespace DailyCodingProblem
{
    /// <summary>
    /// Given the head to a singly linked list, 
    /// where each node also has a “random” pointer that points to anywhere in the linked list, 
    /// deep clone the list.
    /// </summary>
    public class D00966
    {
        public static void Exection()
        {
            var node = new Node() { Value = 1 };
            var dic = new Dictionary<Node,Node>();
            var curr = node;
            while (curr != null)
            {
                var tmp = new Node(){ Value = curr.Value };
                dic.Add(curr, tmp);
                curr = curr.Next;
            }
            curr = node;
            while (curr != null)
            {
                var t = dic[curr];
                var n = curr.Next;
                if (n != null)
                {
                    var nn = dic[n];
                    t.Next = nn;
                } 

                var r = curr.Random;
                if (r != null)
                {
                    var rr = dic[r];
                    t.Random = rr; 
                } 
                curr = curr.Next;
            }
            var result = dic[node];
            Console.WriteLine(result?.Value); 
        }

        public class Node
        {
            public int Value { get; set; }
            public Node Next { get; set; }
            public Node Random { get; set; }
        }
    }
}