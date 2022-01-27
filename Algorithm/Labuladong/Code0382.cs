namespace Labuladong;
public class Code0382
{
    public static void Exection()
    {
    }

    public static int GetRandNode(BaseTree root)
    {
        var res = 0;
        var i = 0;
        var rand = new Random();
        while (root != null)
        {
            i++;
            if (0 == rand.Next(i))
            {
                res = root.Value;
            }
            root = root.Next;
        }
        return res;
    }

    public static int[] GetRandNode(BaseTree root, int k)
    {
        var nums = new int[k];
        var rand = new Random();
        for (int i = 0; i < k && root != null; i++)
        {
            nums[i] = root.Value;
            root = root.Next;
        }

        var ii = k;
        while (root != null)
        {
            ii++;
            var t = rand.Next(ii);
            if (t < k)
            {
                nums[t] = root.Value;
            }
            root = root.Next;
        }

        return nums;
    }
}