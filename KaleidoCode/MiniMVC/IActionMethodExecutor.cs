using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;

namespace KaleidoCode.MiniMVC;

public interface IActionMethodExecutor
{
    object? Execute(object controller, ActionDescriptor actionDescriptor, object?[] arguments);
}

public class ActionMethodExecutor : IActionMethodExecutor
{
    private readonly ConcurrentDictionary<MethodInfo, Func<object, object?[], object?>> _executors = new();
    public object? Execute(object controller, ActionDescriptor actionDescriptor, object?[] arguments)
        => _executors.GetOrAdd(actionDescriptor.MethodInfo, CreateExecutor).Invoke(controller, arguments);
    private Func<object, object?[], object?> CreateExecutor(MethodInfo methodInfo)
    {
        var controller = Expression.Parameter(typeof(object));
        var arguments = Expression.Parameter(typeof(object?[]));

        var parameters = methodInfo.GetParameters();
        var convertedArguments = new Expression[parameters.Length];
        for (int index = 0; index < parameters.Length; index++)
        {
            convertedArguments[index] = Expression.Convert(Expression.ArrayIndex(arguments, Expression.Constant(index)), parameters[index].ParameterType);
        }

        var convertedController = Expression.Convert(controller, methodInfo.DeclaringType!);
        var call = Expression.Call(convertedController, methodInfo, convertedArguments);
        return Expression.Lambda<Func<object, object?[], object?>>(call, controller, arguments).Compile();
    }
}