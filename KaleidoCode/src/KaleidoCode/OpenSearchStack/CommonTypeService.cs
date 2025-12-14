
namespace OpenSearchStack;

public class CommonTypeService
{
    public static void CommonTypeDemo()
    {
        // time
        {
            var unitString = new Time("2d");
            var unitComposed = new Time(2, OpenSearch.Client.TimeUnit.Day);
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
        // distance
        {
            var unitComposed = new Distance(25);
            var unitComposedWithUnits = new Distance(25, DistanceUnit.Meters);
            Console.WriteLine(unitComposed);// 25m
            Distance distanceString = "25";
            Distance distanceStringWithUnits = "25m";
            Console.WriteLine(distanceString);// 25m

            var mm2 = new Distance(2, DistanceUnit.Millimeters);
            Console.WriteLine(mm2);// 2mm
            var cm = new Distance(123.456, DistanceUnit.Centimeters);
            Console.WriteLine(cm);// 123.456cm
            var km = new Distance(0.1, DistanceUnit.Kilometers);
            Console.WriteLine(km);//0.1km
        }
        // date-month
        {
            var date1 = DateMath.Anchored(new DateTime(2015, 05, 05));
            Console.WriteLine(date1);//2015-05-05T00:00:00
            var now1d = DateMath.Now.Add("1d");
            Console.WriteLine(now1d);//now+1d
            var now1d1m = DateMath.Now.Add("1d")
                .Subtract(TimeSpan.FromMinutes(1))
                .RoundTo(DateMathTimeUnit.Day);
            Console.WriteLine(now1d1m);//now+1d-1m\d
            var anchorrang = DateMath.Anchored(new DateTime(2015, 05, 05))
                .Add("1d")
                .Subtract(TimeSpan.FromMinutes(1));
            Console.WriteLine(anchorrang);//2015-05-05T00:00:00||+1d-1m
            var now2s = DateMath.Now.Add(new DateMathTime("2.5s", MidpointRounding.ToEven));
            var now3s = DateMath.Now.Add(new DateMathTime("2.5s", MidpointRounding.AwayFromZero));
            Console.WriteLine(now2s);//now+2s

            DateMathTime twoSeconds = new(2, DateMathTimeUnit.Second);
            DateMathTime twoSecondsFromString = "2s";
            DateMathTime twoSecondsFromTimeSpan = TimeSpan.FromSeconds(2);
            DateMathTime twoSecondsFromDouble = 2000;
            DateMathTime threeSecondsFromString = "3s";
            DateMathTime oneMinuteFromTimeSpan = TimeSpan.FromMinutes(1);
            DateMathTime oneYear = new(1, DateMathTimeUnit.Year);
            DateMathTime oneYearFromString = "1y";
            DateMathTime twelveMonths = new(12, DateMathTimeUnit.Month);
            DateMathTime twelveMonthsFromString = "12M";
            DateMathTime thirteenMonths = new(13, DateMathTimeUnit.Month);
            DateMathTime thirteenMonthsFromString = "13M";
            DateMathTime fiftyTwoWeeks = "52w";
            Console.WriteLine(thirteenMonths);//13M
        }
        {
#if DEBUG
            var tcpStatistics = TcpStats.GetActiveTcpConnections();
            var ipv4Stats = TcpStats.GetTcpStatistics(NetworkInterfaceComponent.IPv4);
            var ipv6Stats = TcpStats.GetTcpStatistics(NetworkInterfaceComponent.IPv6);
#endif
        }
    }
}