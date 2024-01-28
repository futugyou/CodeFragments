
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

}