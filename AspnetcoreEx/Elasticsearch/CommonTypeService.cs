using System.Reflection;
using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class CommonTypeService
{
    public static void CommonTypeDemo()
    {
        var unitString = new Time("2d");
        var unitComposed = new Time(2, Nest.TimeUnit.Day);
        var unitTimeSpan = new Time(TimeSpan.FromDays(2));
        var unitMilliseconds = new Time(1000 * 60 * 60 * 24 * 2);
        Console.WriteLine(unitComposed);// 2d

        Time oneMinute = "1m";
        Time fourteenDays = TimeSpan.FromDays(14);
        Time twoDays = 1000 * 60 * 60 * 24 * 2;
        Console.WriteLine(fourteenDays);// 14d

        var minusOne = Time.MinusOne;
        Console.WriteLine(minusOne);// -1
    }
}