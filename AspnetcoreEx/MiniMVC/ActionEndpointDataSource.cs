using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace AspnetcoreEx.MiniMVC;

public class ActionEndpointDataSource : EndpointDataSource
{   
    private readonly List<(string RouteName, string Template, RouteValueDictionary? Defaults, IDictionary<string, object?>? Constraints, RouteValueDictionary? DataTokens, List<Action<EndpointBuilder>> Conventions, List<Action<EndpointBuilder>> FinallyConventions)> _conventionalRoutes = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IActionDescriptorCollectionProvider _actions;
    private readonly RoutePatternTransformer _transformer;
    private readonly List<Action<EndpointBuilder>> _conventions = new();
    private readonly List<Action<EndpointBuilder>> _finallyConventions = new();
    private readonly EndpointConventionBuilder DefaultBuilder;
    private int _routeOrder;

    private List<Endpoint>? _endpoints;
 
    public ActionEndpointDataSource(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _actions = serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
        _transformer = serviceProvider.GetRequiredService<RoutePatternTransformer>();
        DefaultBuilder = new EndpointConventionBuilder(_conventions, _finallyConventions);
    }

    public override IReadOnlyList<Endpoint> Endpoints => _endpoints ??= _actions.ActionDescriptors.SelectMany(CreateEndpoints).ToList();
    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;

    private IEnumerable<Endpoint> CreateEndpoints(ActionDescriptor actionDescriptor)
    {
        var routeValues = new RouteValueDictionary
        {
            {"controller", actionDescriptor.ControllerName },
            { "action", actionDescriptor.ActionName }
        };
        var attributes = actionDescriptor.MethodInfo.GetCustomAttributes(true).Union(actionDescriptor.MethodInfo.DeclaringType!.GetCustomAttributes(true));
        var routeTemplateProvider = actionDescriptor.RouteTemplateProvider;
        if (routeTemplateProvider is null)
        {
            foreach (var endpoint in CreateConventionalEndpoints(actionDescriptor, routeValues, attributes))
            {
                yield return endpoint;
            }
        }
        else
        {
           yield return  CreateAttributeEndpoint(actionDescriptor, routeValues, attributes);
        }
    }


    private IEnumerable<Endpoint> CreateConventionalEndpoints(ActionDescriptor actionDescriptor, RouteValueDictionary routeValues, IEnumerable<object> attributes)
    {
        foreach (var (routeName, template, defaults, constraints, dataTokens, conventionals, finallyConventionals) in _conventionalRoutes)
        {
            var pattern = RoutePatternFactory.Parse(template, defaults, constraints);
            pattern = _transformer.SubstituteRequiredValues(pattern, routeValues);
            if (pattern is not null)
            {
                var builder = new RouteEndpointBuilder(requestDelegate: HandleRequestAsync, routePattern: pattern, _routeOrder++)
                {
                    ApplicationServices = _serviceProvider
                };
                builder.Metadata.Add(actionDescriptor);
                foreach (var attribute in attributes)
                {
                    builder.Metadata.Add(attribute);
                }
                yield return builder.Build();
            }
        }
    }


    private Endpoint CreateAttributeEndpoint(ActionDescriptor actionDescriptor, RouteValueDictionary routeValues, IEnumerable<object> attributes)
    {
        var routeTemplateProvider = actionDescriptor.RouteTemplateProvider!;
        var pattern = RoutePatternFactory.Parse(routeTemplateProvider.Template!);
        var builder = new RouteEndpointBuilder(requestDelegate: HandleRequestAsync, routePattern: pattern, _routeOrder++)
        {
            ApplicationServices = _serviceProvider
        };
        builder.Metadata.Add(actionDescriptor);
        foreach (var attribute in attributes)
        {
            builder.Metadata.Add(attribute);
        }
        if (routeTemplateProvider is IActionHttpMethodProvider httpMethodProvider)
        {
            builder.Metadata.Add(new HttpMethodActionConstraint(httpMethodProvider.HttpMethods));
        }
        return builder.Build();
    }

    public IEndpointConventionBuilder AddRoute(string routeName, string pattern, RouteValueDictionary? defaults, IDictionary<string, object?>? constraints, RouteValueDictionary? dataTokens)
    {
        var conventions = new List<Action<EndpointBuilder>>();
        var finallyConventions = new List<Action<EndpointBuilder>>();
        _conventionalRoutes.Add((routeName, pattern, defaults, constraints, dataTokens, conventions, finallyConventions));
        return new EndpointConventionBuilder(conventions, finallyConventions);
    }

    private static Task HandleRequestAsync(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint() ?? throw new InvalidOperationException("No endpoint is matched to the current request.");
        var actionDescriptor = endpoint.Metadata.GetMetadata<ActionDescriptor>() ?? throw new InvalidOperationException("No ActionDescriptor is attached to the endpoint as metadata.");
        var actionContext = new ActionContext(httpContext, actionDescriptor);
        return httpContext.RequestServices.GetRequiredService<IActionInvokerFactory>().CreateInvoker(actionContext).InvokeAsync();
    }

    private sealed class EndpointConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<Action<EndpointBuilder>> _conventions;
        private readonly List<Action<EndpointBuilder>> _finallyConventions;

        public EndpointConventionBuilder(List<Action<EndpointBuilder>> conventions, List<Action<EndpointBuilder>> finallyConventions)
        {
            _conventions = conventions;
            _finallyConventions = finallyConventions;
        }

        public void Add(Action<EndpointBuilder> convention) => _conventions.Add(convention);
        public void Finally(Action<EndpointBuilder> finallyConvention) => _finallyConventions.Add(finallyConvention);
    }
}