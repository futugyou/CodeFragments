namespace Labuladong;

public class Code0528
{
    private static int[] preNums;
    private static Random rand = new Random();
    public static void Exection()
    {
        var w = new int[] { 1, 3, 2, 1 };
        var n = w.Length;
        preNums = new int[n + 1];
        preNums[0] = 0;
        for (int i = 1; i <= n; i++)
        {
            preNums[i] = preNums[i - 1] + w[i - 1];
        }
        var r = PickIndex();
    }

    public static int PickIndex()
    {
        int n = preNums.Length;
        int target = rand.Next(1, preNums[n - 1]);
        int left = 0;
        int right = n - 1;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            if (preNums[mid] == target)
            {
                right = mid - 1;
            }
            else if (preNums[mid] > target)
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }
        if (left >= n) return -1;
        return left;
    }
}