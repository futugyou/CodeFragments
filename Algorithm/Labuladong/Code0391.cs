namespace Labuladong;
public class Code0391
{
    public static void Exection()
    {
        var nums = new int[5][] {
            new int[]{1, 1, 3, 3 },
            new int[]{3, 1, 4, 2 },
            new int[]{3, 2, 4, 4 },
            new int[]{1, 3, 2, 4 },
            new int[]{2, 3, 3, 4 } };
        bool result = CheckRectangle(nums);
        Console.WriteLine(result);
    }

    private static bool CheckRectangle(int[][] nums)
    {
        var leftX = int.MaxValue;
        var leftY = int.MaxValue;
        var rightX = int.MinValue;
        var rightY = int.MinValue;
        var area = 0;
        var areaexec = 0;
        var dic = new HashSet<(int, int)>();
        foreach (var point in nums)
        {
            var a = point[0];
            var b = point[1];
            var c = point[2];
            var d = point[3];
            rightY = Math.Max(rightY, d);
            rightX = Math.Max(rightX, c);
            leftY = Math.Min(leftY, b);
            leftX = Math.Min(leftX, b);
            areaexec += (d - b) * (c - a);
            if (dic.Contains((a, b)))
            {
                dic.Remove((a, b));
            }
            else
            {
                dic.Add((a, b));
            }
            if (dic.Contains((c, d)))
            {
                dic.Remove((c, d));
            }
            else
            {
                dic.Add((c, d));
            }
            if (dic.Contains((a, d)))
            {
                dic.Remove((a, d));
            }
            else
            {
                dic.Add((a, d));
            }
            if (dic.Contains((c, b)))
            {
                dic.Remove((c, b));
            }
            else
            {
                dic.Add((c, b));
            }
        }
        area = (rightY - leftX) * (rightX - leftY);
        if (area != areaexec)
        {
            return false;
        }
        if (dic.Count != 4)
        {
            return false;
        }
        if (!dic.Contains((leftX, leftY))
        || !dic.Contains((leftX, rightY))
        || !dic.Contains((rightX, rightY))
        || !dic.Contains((rightX, leftY))
        )
        {
            return false;
        }
        return true;
    }
}