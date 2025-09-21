using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace MinimizeAspnet.MiniMVC;

public class ActionDescriptor
{
    public MethodInfo MethodInfo { get; }
    public IRouteTemplateProvider? RouteTemplateProvider { get; }
    public string ControllerName { get; }
    public string ActionName { get; }
    public ParameterDescriptor[] Parameters { get; }
    public ActionDescriptor(
        MethodInfo methodInfo,
        IRouteTemplateProvider? routeTemplateProvider)
    {
        MethodInfo = methodInfo;
        RouteTemplateProvider = routeTemplateProvider;
        ControllerName = MethodInfo.DeclaringType!.Name;
        ControllerName = ControllerName[..^"Controller".Length];
        ActionName = MethodInfo.Name;
        Parameters = [.. methodInfo.GetParameters().Select(it => new ParameterDescriptor(it))];
    }
}

public class ParameterDescriptor(ParameterInfo parameterInfo)
{
    public ParameterInfo ParameterInfo
        => parameterInfo;
}