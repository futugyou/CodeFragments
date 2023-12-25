using Microsoft.AspNetCore.Http;

namespace AspnetcoreEx.MiniMVC;

public interface IActionInvoker
{
    Task InvokeAsync();
}

public interface IActionInvokerFactory
{
    IActionInvoker CreateInvoker(ActionContext actionContext);
}

public class ActionContext(HttpContext httpContext, ActionDescriptor actionDescriptor)
{
    public HttpContext HttpContext => httpContext;

    public ActionDescriptor ActionDescriptor => actionDescriptor;
}