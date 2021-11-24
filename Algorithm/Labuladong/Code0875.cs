namespace Labuladong;

public class Code0875
{
    public static void Exection()
    {
        var piles = new int[] { 30, 11, 23, 4, 20 };
        var h = 6;
        var a = MinSpeed(piles, h);
        Console.WriteLine(a);
    }

    private static int MinSpeed(int[] piles, int h)
    {
        if (h < piles.Length)
        {
            return -1;
        }
        var maxspeed = piles.Max();
        if (h == piles.Length)
        {
            return maxspeed;
        }
        var left = 1;
        var right = piles.Max();

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            int hour = ExecHour(piles, mid);
            if (hour > h)
            {
                left = mid + 1;
            }
            else if (hour < h)
            {
                right = mid - 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        if (left > maxspeed)
        {
            left = maxspeed;
        }
        return left;
    }

    private static int ExecHour(int[] piles, int mid)
    {
        var result = 0;
        for (int i = 0; i < piles.Length; i++)
        {
            result += piles[i] / mid;
            if (piles[i] % mid > 0)
            {
                result++;
            }
        }
        return result;
    }
}