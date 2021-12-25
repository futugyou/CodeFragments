
namespace Labuladong;
public class Code0921
{
    public static void Exection()
    {
        Console.WriteLine(MinAddToMakeValid(")("));
    }

    public static int MinAddToMakeValid(string str)
    {
        var insert = 0;
        var right = 0;
        foreach (var s in str)
        {
            if (s == '(')
            {
                right++;
            }
            else
            {
                if (right == 0)
                {
                    insert++;
                }
                else
                {
                    right--;
                }
            }
        }
        return insert + right;
    }
}