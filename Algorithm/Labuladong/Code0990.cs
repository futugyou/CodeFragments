namespace Labuladong;

public class Code0990
{
    public static void Exection()
    {
        var str = new string[] { "a==b", "b!=a" };
        Console.WriteLine(EquationsPossible(str));
    }
    public static bool EquationsPossible(string[] equations)
    {
        var uf = new UnionFind(26);
        foreach (var item in equations)
        {
            if (item[1] == '=')
            {
                var a = item[0];
                var b = item[3];
                uf.Union(a - 'a', b - 'a');
            }
        }
        foreach (var item in equations)
        {
            if (item[1] == '!')
            {
                var a = item[0];
                var b = item[3];
                if (uf.Connected(a - 'a', b - 'a'))
                {
                    return false;
                }
            }
        }
        return true;
    }
}