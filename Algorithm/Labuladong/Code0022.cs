using System.Text;

namespace Labuladong;
public class Code0022
{
    public static void Exection()
    {
        List<string> result = GenerateParenthesis(3);
        Console.WriteLine(string.Join(", ", result));
    }

    private static List<string> GenerateParenthesis(int v)
    {
        List<string> result = new List<string>();
        StringBuilder sb = new StringBuilder();
        BackTrack(v, v, sb, result);
        return result;
    }

    private static void BackTrack(int left, int right, StringBuilder sb, List<string> result)
    {
        if (left > right || left < 0 || right < 0)
        {
            return;
        }
        if (left == 0 && right == 0)
        {
            result.Add(sb.ToString());
            return;
        }
        sb.Append("(");
        BackTrack(left - 1, right, sb, result);
        sb.Remove(sb.Length - 1, 1);

        sb.Append(")");
        BackTrack(left, right - 1, sb, result);
        sb.Remove(sb.Length - 1, 1);
    }
}