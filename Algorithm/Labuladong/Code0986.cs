namespace Labuladong;
public class Code0986
{
    public static void Exection()
    {
        var firstList = new int[,] { { 0, 2 }, { 5, 10 }, { 13, 23 }, { 24, 25 } };
        var secondList = new int[,] { { 1, 5 }, { 8, 12 }, { 15, 24 }, { 25, 26 } };
        var result = new List<int[]>();
        int i = 0;
        var j = 0;
        while (i < firstList.Length / 2 && j < secondList.Length / 2)
        {
            var a1 = firstList[i, 0];
            var a2 = firstList[i, 1];
            var b1 = secondList[j, 0];
            var b2 = secondList[j, 1];
            if (a1 <= b2 && a2 >= b1)
            {
                result.Add(new int[] { Math.Max(a1, b1), Math.Min(a2, b2) });
            }
            if (a2 < b2)
            {
                i++;
            }
            else
            {
                j++;
            }
        }
    }
}