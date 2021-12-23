namespace Labuladong;

public class Code0370
{
    public static void Exection()
    {

    }

    public static int[] GetModifiedArray(int length, int[][] updates)
    {
        var nums = new int[length];
        var diff = new Difference(nums);
        foreach (var item in updates)
        {
            diff.Increment(item[0], item[1], item[2]);
        }
        return diff.Result();
    }
}