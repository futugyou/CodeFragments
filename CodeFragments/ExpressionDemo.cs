
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

    public static void Interpreting()
    {
        {
            Expression<Func<int, bool>> exprTree = num => num < 5;
            // Decompose the expression tree.
            ParameterExpression param = exprTree.Parameters[0];
            BinaryExpression operation = (BinaryExpression)exprTree.Body;
            ParameterExpression left = (ParameterExpression)operation.Left;
            ConstantExpression right = (ConstantExpression)operation.Right;

            Console.WriteLine("Decomposed expression: {0} => {1} {2} {3}", param.Name, left.Name, operation.NodeType, right.Value);
            Console.WriteLine();
        }

        {
            var constant = Expression.Constant(24, typeof(int));

            Console.WriteLine($"This is a/an {constant.NodeType} expression type");
            Console.WriteLine($"The type of the constant value is {constant.Type}");
            Console.WriteLine($"The value of the constant value is {constant.Value}");
            Console.WriteLine();
        }

        {
            Expression<Func<int, int, int>> addition = (a, b) => a + b;
            Console.WriteLine($"This expression is a {addition.NodeType} expression type");
            Console.WriteLine($"The name of the lambda is {((addition.Name == null) ? "<null>" : addition.Name)}");
            Console.WriteLine($"The return type is {addition.ReturnType.ToString()}");
            Console.WriteLine($"The expression has {addition.Parameters.Count} arguments. They are:");
            foreach (var argumentExpression in addition.Parameters)
            {
                Console.WriteLine($"\tParameter Type: {argumentExpression.Type.ToString()}, Name: {argumentExpression.Name}");
            }

            var additionBody = (BinaryExpression)addition.Body;
            Console.WriteLine($"The body is a {additionBody.NodeType} expression");
            Console.WriteLine($"The left side is a {additionBody.Left.NodeType} expression");
            var left = (ParameterExpression)additionBody.Left;
            Console.WriteLine($"\tParameter Type: {left.Type.ToString()}, Name: {left.Name}");
            Console.WriteLine($"The right side is a {additionBody.Right.NodeType} expression");
            var right = (ParameterExpression)additionBody.Right;
            Console.WriteLine($"\tParameter Type: {right.Type.ToString()}, Name: {right.Name}");
            Console.WriteLine();
        }
    }

    public static void CustomVisitor()
    {
        Expression<Func<int, int, int>> sum3 = (a, b) => (1 + a) + (3 + b);
        var v = Visitor.CreateFromExpression(sum3);
        v.Visit("");
    }

    public static void StandardVisitor()
    {
        Expression<Func<int>> sum3 = () => 1;
        var v = new StandardVisitor();
        v.Visit(sum3);
    }

    public static void Loop()
    {
        var nArgument = Expression.Parameter(typeof(int), "n");
        var result = Expression.Variable(typeof(int), "result");

        // Creating a label that represents the return value
        LabelTarget label = Expression.Label(typeof(int));

        var initializeResult = Expression.Assign(result, Expression.Constant(1));

        // This is the inner block that performs the multiplication,
        // and decrements the value of 'n'
        var block = Expression.Block(
            Expression.Assign(result,
                Expression.Multiply(result, nArgument)),
            Expression.PostDecrementAssign(nArgument)
        );

        // Creating a method body.
        BlockExpression body = Expression.Block(
            new[] { result },
            initializeResult,
            Expression.Loop(
                Expression.IfThenElse(
                    Expression.GreaterThan(nArgument, Expression.Constant(1)),
                    block,
                    Expression.Break(label, result)
                ),
                label
            )
        );

        Expression<Func<int, int>> le = Expression.Lambda<Func<int, int>>(body, nArgument);
        Func<int, int> compiledExpression = le.Compile();
        int result1 = compiledExpression(4);
        Console.WriteLine(result1);
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