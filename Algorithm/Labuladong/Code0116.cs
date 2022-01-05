namespace Labuladong;
public class Code0116
{
    public static void Exection()
    {

    }
    public static void ConnectTree(BaseTree root)
    {
        if (root == null)
        {
            return;
        }
        Queue<BaseTree> q = new Queue<BaseTree>();
        q.Enqueue(root);
        while (q.Any())
        {
            var size = q.Count;
            BaseTree curr = null;
            BaseTree prew = null;
            for (int i = 0; i < size; i++)
            {
                curr = q.Dequeue();
                if (prew != null)
                {
                    prew.Next = curr;
                }
                prew = curr;
                if (curr.Left != null)
                {
                    q.Enqueue(curr.Left);
                    q.Enqueue(curr.Right);
                }
            }
        }
    }
    public static void ConnectTree2(BaseTree root)
    {
        if (root == null)
        {
            return;
        }
        InnerConnect(root.Left, root.Right);
    }

    private static void InnerConnect(BaseTree left, BaseTree right)
    {
        if (left == null || right == null)
        {
            return;
        }
        left.Next = right;
        InnerConnect(left.Left, left.Right);
        InnerConnect(right.Left, right.Right);
        InnerConnect(left.Right, right.Left);
    }
}