namespace Labuladong;

public class Code0042
{
    public static void Exection()
    {
        var height = new int[] { 0, 1, 0, 2, 1, 0, 1, 3, 2, 1, 2, 1 };
        var l = height.Length;
        var res = 0;
        var lmax = new int[l];
        var rmax = new int[l];
        lmax[0] = height[0];
        rmax[l - 1] = height[l - 1];
        for (int i = 1; i < l; i++)
        {
            lmax[i] = Math.Max(height[i], lmax[i - 1]);
        }
        for (int i = l - 2; i >= 0; i--)
        {
            rmax[i] = Math.Max(height[i], rmax[i + 1]);
        }
        for (int i = 1; i < l - 1; i++)
        {
            res += Math.Min(lmax[i], rmax[i]) - height[i];
        }
        Console.WriteLine(res);


        res = 0;
        var left = 0;
        var right = l - 1;
        var l_max = height[0];
        var r_max = height[l - 1];
        while (left <= right)
        {
            l_max = Math.Max(l_max, height[left]);
            r_max = Math.Max(r_max, height[right]);
            if (l_max < r_max)
            {
                res += l_max - height[left];
                left++;
            }
            else
            {
                res += r_max - height[right];
                right--;
            }
        }
        Console.WriteLine(res);
    }
}