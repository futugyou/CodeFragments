using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace KaleidoCode.MiniMVC;

public interface IArgumentBinder
{
    public ValueTask<object?> BindAsync(ActionContext actionContext, ParameterDescriptor parameterDescriptor);
}

public class ArgumentBinder : IArgumentBinder
{
    private readonly ConcurrentDictionary<Type, object?> _defaults = new();
    private readonly MethodInfo _method = typeof(ArgumentBinder).GetMethod(nameof(GetDefaultValue))!;
    public ValueTask<object?> BindAsync(ActionContext actionContext, ParameterDescriptor parameterDescriptor)
    {
        var requestServices = actionContext.HttpContext.RequestServices;
        var parameterInfo = parameterDescriptor.ParameterInfo;
        var parameterName = parameterInfo.Name!;
        var parameterType = parameterInfo.ParameterType;

        // From registered service
        var result = requestServices.GetService(parameterType);
        if (result is not null)
        {
            return ValueTask.FromResult(result)!;
        }

        // From query, route, body
        var request = actionContext.HttpContext.Request;
        if (request.Query.TryGetValue(parameterName, out var value1))
        {
            return ValueTask.FromResult(Convert.ChangeType((string)value1!, parameterType))!;
        }
        if (request.RouteValues.TryGetValue(parameterName, out var value2))
        {
            return ValueTask.FromResult(Convert.ChangeType(value2, parameterType)!)!;
        }
        if (request.ContentLength > 0)
        {
            return JsonSerializer.DeserializeAsync(request.Body, parameterType);
        }

        // From default value
        var defaultValue = _defaults.GetOrAdd(parameterType, type => _method.MakeGenericMethod(parameterType).Invoke(null, null));
        return ValueTask.FromResult(defaultValue);
    }
    public static T GetDefaultValue<T>() => default!;
}