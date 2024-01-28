
using System.Linq.Expressions;

namespace CodeFragments;
public class ExpressionDemo
{
    static readonly Expression<Func<int, int>> addFive = (num) => num + 5;

    public static void Base()
    {
        if (addFive is LambdaExpression lambdaExp)
        {
            foreach (var parameter in lambdaExp.Parameters)
            {
                Console.WriteLine(parameter.Name);
                Console.WriteLine(parameter.Type);
            }
        }

        // Addition is an add expression for "1 + 2"
        var one = Expression.Constant(1, typeof(int));
        var two = Expression.Constant(2, typeof(int));
        var addition = Expression.Add(one, two);
        Expression<Func<int>> le = Expression.Lambda<Func<int>>(addition);
        Func<int> compiledExpression = le.Compile();
        int result = compiledExpression();
        Console.WriteLine(result);
    }
    public static void ClosureError()
    {
        var func = CreateBoundResource();
        try
        {
            var r = func(1);
            Console.WriteLine(r);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static Func<int, int> CreateBoundResource()
    {
        using (var constant = new Resource()) // constant is captured by the expression tree
        {
            Expression<Func<int, int>> expression = (b) => constant.Argument + b;
            var rVal = expression.Compile();
            return rVal;
        }
    }
}

public class Resource : IDisposable
{
    private bool _isDisposed = false;
    public int Argument
    {
        get
        {
            if (!_isDisposed)
                return 5;
            else throw new ObjectDisposedException("Resource");
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
    }
}