using System.Collections.Concurrent;
using System.Reflection;
using System.Linq.Expressions;

namespace KaleidoCode.MiniMVC;

public interface IActionResultConverter
{
    ValueTask<IActionResult> ConvertAsync(object? result);
}

public class ActionResultConverter : IActionResultConverter
{
    private readonly MethodInfo _valueTaskConvertMethod = typeof(ActionResultConverter).GetMethod(nameof(ConvertFromValueTask))!;
    private readonly MethodInfo _taskConvertMethod = typeof(ActionResultConverter).GetMethod(nameof(ConvertFromTask))!;
    private readonly ConcurrentDictionary<Type, Func<object, ValueTask<IActionResult>>> _converters = new();

    public ValueTask<IActionResult> ConvertAsync(object? result)
    {
        // Null
        if (result is null)
        {
            return ValueTask.FromResult<IActionResult>(VoidActionResult.Instance);
        }

        // Task<IActionResult>
        if (result is Task<IActionResult> taskOfActionResult)
        {
            return new ValueTask<IActionResult>(taskOfActionResult);
        }

        // ValueTask<IActionResult>
        if (result is ValueTask<IActionResult> valueTaskOfActionResult)
        {
            return valueTaskOfActionResult;
        }

        // IActionResult
        if (result is IActionResult actionResult)
        {
            return ValueTask.FromResult(actionResult);
        }

        // ValueTask
        if (result is ValueTask valueTask)
        {
            return Convert(valueTask);
        }

        // Task
        var type = result.GetType();
        if (type == typeof(Task))
        {
            return Convert((Task)result);
        }

        // ValueTask<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            return _converters.GetOrAdd(type, t => CreateValueTaskConverter(t, _valueTaskConvertMethod)).Invoke(result);
        }

        // Task<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return _converters.GetOrAdd(type, t => CreateValueTaskConverter(t, _taskConvertMethod)).Invoke(result);
        }

        // Object
        return ValueTask.FromResult<IActionResult>(new ObjectActionResult(result));
    }

    public static async ValueTask<IActionResult> ConvertFromValueTask<T>(ValueTask<T> valueTask)
    {
        var result = valueTask.IsCompleted ? valueTask.Result : await valueTask;
        return result is IActionResult actionResult ? actionResult : new ObjectActionResult(result!);
    }

    public static async ValueTask<IActionResult> ConvertFromTask<T>(Task<T> task)
    {
        var result = await task;
        return result is IActionResult actionResult ? actionResult : new ObjectActionResult(result!);
    }

    private static async ValueTask<IActionResult> Convert(ValueTask valueTask)
    {
        if (!valueTask.IsCompleted) await valueTask;
        return VoidActionResult.Instance;
    }

    private static async ValueTask<IActionResult> Convert(Task task)
    {
        await task;
        return VoidActionResult.Instance;
    }

    private static Func<object, ValueTask<IActionResult>> CreateValueTaskConverter(Type valueTaskType, MethodInfo convertMethod)
    {
        var parameter = Expression.Parameter(typeof(object));
        var convert = Expression.Convert(parameter, valueTaskType);
        var method = convertMethod.MakeGenericMethod(valueTaskType.GetGenericArguments()[0]);
        var call = Expression.Call(method, convert);
        return Expression.Lambda<Func<object, ValueTask<IActionResult>>>(call, parameter).Compile();
    }

    private sealed class VoidActionResult : IActionResult
    {
        public static readonly VoidActionResult Instance = new();
        public Task ExecuteResultAsync(ActionContext actionContext) => Task.CompletedTask;
    }

    private sealed class ObjectActionResult(object result) : IActionResult
    {
        public Task ExecuteResultAsync(ActionContext actionContext)
        {
            var response = actionContext.HttpContext.Response;
            response.ContentType = "text/plain";
            return response.WriteAsync(result.ToString()!);
        }
    }
}