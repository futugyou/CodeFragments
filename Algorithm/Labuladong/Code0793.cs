namespace Labuladong;

public class Code0793
{
    public static void Exection()
    {
        int x = 5;
        //if 1, it means no filter
        int result = PreimageSizeFZF(x);
        Console.WriteLine(result);
    }

    public static int PreimageSizeFZF(int x)
    {
        return (int)(RightBound(x) - LeftBound(x) + 1);
    }

    private static long TrailingZeroes(long n)
    {
        long res = 0;
        for (long i = n; i / 5 > 0; i = i / 5)
        {
            res += i / 5;
        }
        return res;
    }

    private static long LeftBound(int x)
    {
        long left = 0;
        long right = long.MaxValue - 1;
        while (left <= right)
        {
            long mid = left + (right - left) / 2;
            if (TrailingZeroes(mid) > x)
            {
                right = mid - 1;
            }
            else if (TrailingZeroes(mid) == x)
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        if (left == long.MaxValue || TrailingZeroes(left) != x)
        {
            return 0;
        }
        return left;
    }

    private static long RightBound(int x)
    {
        long left = 0;
        long right = long.MaxValue - 1;
        while (left <= right)
        {
            long mid = left + (right - left) / 2;
            if (TrailingZeroes(mid) > x)
            {
                right = mid - 1;
            }
            else if (TrailingZeroes(mid) == x)
            {
                left = mid + 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        if (right < 0 || TrailingZeroes(right) != x)
        {
            return 0;
        }
        return right;
    }
}