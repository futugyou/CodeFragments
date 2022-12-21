using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Http;

namespace AspnetcoreEx.RouteEx;

public static class RouteExtensions
{
    public static IServiceCollection AddRouteExtension(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Configure<RouteOptions>(options => 
        {
           options.ConstraintMap("culture", typeof(CultureConstraint));
        });
        return services;
    }


    public static WebApplication RoutePatternFactoryExtension(this WebApplication app)
    {
        var templete = @"weather/{city:regex(^\d{{2,3}}$)=010}/{days:int:range(1,4)=4}/{detailed?}";
        var pattern = RoutePatternFactory.Parse(
            pattern: templete,
            default: null,
            parameterPolicies: null,
            requiredValues = new { city = "010", days = 4},
        );
        
        app.MapGet("/routepattern", () => RoutePatternCase.Format(pattern));
        // http://localhost:5000/routepoint?point=(123,456)
        app.MapGet("/routepoint", (PointForRoute point) => point);

        templete = "resources/{land:culture}/{resourceName:required}";
        app.MapGet("/culture", GetResource);

        app.MapGet("/exception404", ExceptionExtensions.BuilderHandler(app, false));
        app.MapGet("/exception404t", ExceptionExtensions.BuilderHandler(app, true));

        return app;

        IResult GetResource(string lang, string resourceName)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(lang);
            var text = Resources.ResourceManager.GetString(resourceName);
            return string.IsNullOrEmpty(text) ? Results.NotFound() : Results.Content(text);
        }
    }
}