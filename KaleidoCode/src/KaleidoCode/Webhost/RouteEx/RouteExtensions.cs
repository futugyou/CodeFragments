using Microsoft.AspNetCore.Routing.Patterns;

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

        app.MapGet("api/custome/routepattern", () => RoutePatternCase.Format(pattern)).WithTags("Home");
        // point can not be recognized by `swagger`, but `scalar` is ok.
        // http://localhost:5000/routepoint?point=(123,456)
        app.MapGet("api/custome/routepoint", ([FromQuery] PointForRoute point) => point).WithTags("Home");

        // exception404 and exception404t will not be recognized by openapi,
        // because ExceptionExtensions.BuilderHandler is `RequestDelegate` instead of `Delegate`.
        app.MapGet("/notallowed404", ExceptionExtensions.BuilderHandler(app, false)).WithTags("Home");
        app.MapGet("/allowed404", ExceptionExtensions.BuilderHandler(app, true)).WithTags("Home");

        return app;
    }
}