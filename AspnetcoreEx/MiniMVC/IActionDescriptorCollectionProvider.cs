using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AspnetcoreEx.MiniMVC;

public interface IActionDescriptorCollectionProvider
{
    IReadOnlyList<ActionDescriptor> ActionDescriptors { get; }
}

public class ActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
{
    private readonly Assembly _assembly;
    private List<ActionDescriptor>? _actionDescriptors;
    public IReadOnlyList<ActionDescriptor> ActionDescriptors
        => _actionDescriptors
        ??= [.. Resolve(_assembly.GetExportedTypes())];

    public ActionDescriptorCollectionProvider(IWebHostEnvironment environment)
    {
        var assemblyName = new AssemblyName(environment.ApplicationName);
        _assembly = Assembly.Load(assemblyName);
    }

    private IEnumerable<ActionDescriptor> Resolve(IEnumerable<Type> types)
    {
        var methods = types
            .Where(IsValidController)
            .SelectMany(type => type.GetMethods()
                .Where(method => method.DeclaringType == type
                    && IsValidAction(method)));

        foreach (var method in methods)
        {
            var providers = method.GetCustomAttributes()
                .OfType<IRouteTemplateProvider>();
            if (providers.Any())
            {
                foreach (var provider in providers)
                {
                    yield return new ActionDescriptor(method, provider);
                }
            }
            else
            {
                yield return new ActionDescriptor(method, null);
            }
        }
    }

    private static bool IsValidController(Type candidate)
        => candidate.IsPublic
        && !candidate.IsAbstract
        && candidate.Name.EndsWith("Controller");
    private static bool IsValidAction(MethodInfo methodInfo)
        => methodInfo.IsPublic | !methodInfo.IsAbstract;
}