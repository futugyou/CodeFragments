namespace Labuladong;
public class Code1094
{
    public static void Exection()
    {
        var trips = new int[][] { new int[] { 2, 1, 5 }, new int[] { 3, 3, 7 } };
        var cap = 4;
        var r = CanPooling(trips, cap);
        Console.WriteLine(r);
    }

    public static bool CanPooling(int[][] trips, int cap)
    {
        int[] nums = new int[1001];
        var diff = new Difference(nums);
        for (int i = 0; i < trips.Length; i++)
        {
            diff.Increment(trips[i][1], trips[i][2], trips[i][0]);
        }
        var res = diff.Result();
        foreach (var item in res)
        {
            if (item > cap)
            {
                return false;
            }
        }
        return true;
    }
}