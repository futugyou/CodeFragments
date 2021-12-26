
namespace Labuladong;
public class Code1541
{
    public static void Exection()
    {
        Console.WriteLine(MinAddToMakeValid(")()"));
    }

    private static int MinAddToMakeValid(string v)
    {
        var insert = 0;
        var right = 0;
        foreach (var s in v)
        {
            if (s == '(')
            {
                right += 2;
            }
            else
            {
                if (right == 0)
                {
                    insert += 1;
                    right = 1;
                }
                else
                {
                    right -= 1;
                }
            }
        }
        return insert + right;
    }
}
