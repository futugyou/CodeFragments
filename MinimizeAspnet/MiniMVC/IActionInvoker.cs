using Microsoft.AspNetCore.Http;

namespace MinimizeAspnet.MiniMVC;

public class ActionContext(HttpContext httpContext, ActionDescriptor actionDescriptor)
{
    public HttpContext HttpContext => httpContext;

    public ActionDescriptor ActionDescriptor => actionDescriptor;
}

public interface IActionInvoker
{
    Task InvokeAsync();
}

public class ActionInvoker(ActionContext actionContext) : IActionInvoker
{
    public ActionContext ActionContext { get; } = actionContext;

    public async Task InvokeAsync()
    {
        var requestServices = ActionContext.HttpContext.RequestServices;

        // Create controller instance
        var controller = ActivatorUtilities.CreateInstance(requestServices, ActionContext.ActionDescriptor.MethodInfo.DeclaringType!);
        try
        {
            // Bind arguments
            var parameters = ActionContext.ActionDescriptor.Parameters;
            var arguments = new object?[parameters.Length];
            var binder = requestServices.GetRequiredService<IArgumentBinder>();
            for (int index = 0; index < parameters.Length; index++)
            {
                var valueTask = binder.BindAsync(ActionContext, parameters[index]);
                if (valueTask.IsCompleted)
                {
                    arguments[index] = valueTask.Result;
                }
                else
                {
                    arguments[index] = await valueTask;
                }
            }

            // Execute action method
            var executor = requestServices.GetRequiredService<IActionMethodExecutor>();

            var result = executor.Execute(controller, ActionContext.ActionDescriptor, arguments);

            // Convert result to IActionResult
            var converter = requestServices.GetRequiredService<IActionResultConverter>();
            var convert = converter.ConvertAsync(result);
            var actionResult = convert.IsCompleted ? convert.Result : await convert;

            // Execute result
            await actionResult.ExecuteResultAsync(ActionContext);
        }
        finally
        {
            (controller as IDisposable)?.Dispose();
        }
    }
}
