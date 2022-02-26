using System.Reflection;
using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class CommonTypeService
{
    public static void CommonTypeDemo()
    {
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
        {
            var unitComposed = new Distance(25);
            var unitComposedWithUnits = new Distance(25, Nest.DistanceUnit.Meters);
            Console.WriteLine(unitComposed);// 25m
            Distance distanceString = "25";
            Distance distanceStringWithUnits = "25m";
            Console.WriteLine(distanceString);// 25m

            var mm2 = new Distance(2, Nest.DistanceUnit.Millimeters);
            Console.WriteLine(mm2);// 2mm
            var cm = new Distance(123.456, Nest.DistanceUnit.Centimeters);
            Console.WriteLine(cm);// 123.456cm
            var km = new Distance(0.1, Nest.DistanceUnit.Kilometers);
            Console.WriteLine(km);//0.1km
        }
    }
}