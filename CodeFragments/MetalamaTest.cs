using Metalama.Framework.Aspects;

namespace CodeFragments;

public class MetalamaTest
{
    [LogAttribute]
    public int Add(int a, int b)
    {
        var result = a + b;
        Console.WriteLine("Add: " + result);
        return result;
    }
}

public class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " 开始运行.");
        var result = meta.Proceed();
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " 结束运行.");
        return result;
 
    }
}