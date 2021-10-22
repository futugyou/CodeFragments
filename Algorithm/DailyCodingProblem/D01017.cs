namespace DailyCodingProblem;

/// <summary>
/// Given a list, sort it using this method: reverse(lst, i, j), which reverses lst from i to j.
/// </summary>
public class D01017
{
    public static void Exection()
    {
        var num = new int[] { 9, 8, 5, 6, 2, 1 };
        for (int i = 0; i < num.Length; i++)
        {
            var minindex = i;
            var min = num[i];
            for (int j = i + 1; j < num.Length; j++)
            {
                if (min > num[j])
                {
                    minindex = j;
                    min = num[j];
                }
            }
            if (minindex != i)
            {
                Reverse(num, i, minindex);
            }
        }
    }
    public static void Reverse(int[] num, int i, int j)
    {
        while (i <= j)
        {
            var t = num[i];
            num[i] = num[j];
            num[j] = t;
            i++;
            j--;
        }
    }
}