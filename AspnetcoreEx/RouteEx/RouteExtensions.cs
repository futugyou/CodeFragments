using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing.Patterns;

namespace AspnetcoreEx.RouteEx;

public static class RouteExtensions
{
    public static WebApplication RoutePatternFactoryExtension(this WebApplication app)
    {
        var templete = @"weather/{city:regex(^\d{{2,3}}$)=010}/{days:int:range(1,4)=4}/{detailed?}"
        var pattern = RoutePatternFactory.Parse(
            pattern: templete,
            default: null,
            parameterPolicies: null,
            requiredValues = new { city = "010", days = 4},
        );
        app.MapGet("/routepattern", () => RoutePatternCase.Format(pattern));
        return app;
    }
}