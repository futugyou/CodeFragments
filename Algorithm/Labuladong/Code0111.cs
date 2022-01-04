
namespace Labuladong;
public class Code0111
{
    public static void Exection()
    {

    }

    public static int GetMinDepth(BaseTree root)
    {
        if (root == null)
        {
            return 0;
        }
        var queue = new Queue<BaseTree>();
        queue.Enqueue(root);
        int depth = 1;
        while (queue.Any())
        {
            var size = queue.Count;
            for (int i = 0; i < size; i++)
            {
                var curr = queue.Dequeue();
                if (curr.Left == null && curr.Right == null)
                {
                    return depth;
                }
                if (curr.Left != null)
                {
                    queue.Enqueue(curr.Left);
                }
                if (curr.Right != null)
                {
                    queue.Enqueue(curr.Right);
                }
            }
            depth++;
        }
        return depth;
    }
}