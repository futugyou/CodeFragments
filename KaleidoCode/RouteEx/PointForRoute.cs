using System.Reflection;

namespace KaleidoCode.RouteEx;

public class PointForRoute
{
    public int X { get; set; }
    public int Y { get; set; }

    public PointForRoute(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool TryParse(string expression, out PointForRoute? point)
    {
        var split = expression.Trim('(', ')').Split(',');
        if (split.Length == 2 && int.TryParse(split[0], out var x) && int.TryParse(split[1], out var y))
        {
            point = new PointForRoute(x, y);
            return true;
        }
        point = null;
        return false;
    }

    public static ValueTask<PointForRoute?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    {
        PointForRoute? point = null;
        var name = parameter.Name;

        var value = httpContext.GetRouteData().Values.TryGetValue(name!, out var v)
            ? v
            : httpContext.Request.Query[name!].SingleOrDefault();

        if (value is string expression)
        {
            var split = expression.Trim('(', ')').Split(',');
            if (split.Length == 2 && int.TryParse(split[0], out var x) && int.TryParse(split[1], out var y))
            {
                point = new PointForRoute(x, y);
            }
        }

        return new ValueTask<PointForRoute?>(point);
    }
}