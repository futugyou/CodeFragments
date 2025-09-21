
namespace MinimizeAspnet.MiniMVC;

public interface IActionInvokerFactory
{
    IActionInvoker CreateInvoker(ActionContext actionContext);
}

public class ActionInvokerFactory : IActionInvokerFactory
{
    public IActionInvoker CreateInvoker(ActionContext actionContext) => new ActionInvoker(actionContext);
}