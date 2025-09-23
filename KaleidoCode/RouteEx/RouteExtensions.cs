using Microsoft.AspNetCore.Routing.Patterns;
using KaleidoCode.Extensions;

namespace KaleidoCode.RouteEx;

public static class RouteExtensions
{
    public static IServiceCollection AddRouteExtension(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add("culture", typeof(CultureConstraint));
        });
        return services;
    }


    public static WebApplication RoutePatternFactoryExtension(this WebApplication app)
    {
        var template = @"weather/{city:regex(^\d{{2,3}}$)=010}/{days:int:range(1,4)=4}/{detailed?}";
        var pattern = RoutePatternFactory.Parse(
            pattern: template,
            defaults: null,
            parameterPolicies: null,
            requiredValues: new { city = "010", days = 4 }
        );

        app.MapGet("/routepattern", () => RoutePatternCase.Format(pattern));
        // http://localhost:5000/routepoint?point=(123,456)
        app.MapGet("/routepoint", (PointForRoute point) => point);

        app.MapGet("/exception404", ExceptionExtensions.BuilderHandler(app, false));
        app.MapGet("/exception404t", ExceptionExtensions.BuilderHandler(app, true));

        return app;
    }
}