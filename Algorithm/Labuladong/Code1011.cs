namespace Labuladong;

public class Code1011
{
    public static void Exection()
    {
        var weights = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var h = 5;
        var a = MinSpeed(weights, h);
        Console.WriteLine(a);
    }

    private static int MinSpeed(int[] weights, int h)
    {
        var left = weights.Max();
        var right = weights.Sum();
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var t = GetPower(weights, mid);
            if (t > h)
            {
                left = mid + 1;
            }
            else if (t == h)
            {
                right = mid - 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return left;
    }

    private static int GetPower(int[] weights, int mid)
    {
        var res = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            if (mid >= weights[i])
            {
                mid = mid - weights[i];
                res++;
            }
            else
            {
                break;
            }
        }
        return res;
    }
}