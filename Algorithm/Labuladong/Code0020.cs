namespace Labuladong;
public class Code0020
{
    public static void Exection()
    {
        var s = "([)]";
        Console.WriteLine(IsValid(s));
        s = "()[]{}";
        Console.WriteLine(IsValid(s));
    }

    public static bool IsValid(string str)
    {
        var stack = new Stack<char>();
        foreach (var c in str)
        {
            if (c == '[' || c == '(' || c == '{')
            {
                stack.Push(c);
            }
            else
            {
                if (stack.Any() && stack.Peek() == CheckLeft(c))
                {
                    stack.Pop();
                }
                else
                {
                    return false;
                }
            }
        }
        return !stack.Any();
    }

    public static char CheckLeft(char str)
    {
        if (str == ']') return '[';
        if (str == '}') return '{';
        return '(';
    }
}