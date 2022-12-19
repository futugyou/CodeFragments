
namespace AspnetcoreEx.RouteEx;

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
        var split = expression.Trim('(',')').Split(',');
        if (split.Length == 2 && int.TryParse(split[0], out var x) && int.TryParse(split[1], out var y))
        {
            point = new PointForRoute(x, y);
            return true;
        }
        point = null;
        return false;
    }
}