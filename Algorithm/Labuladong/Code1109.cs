namespace Labuladong;
public class Code1109
{
    public static void Exection()
    {
        var bookings = new int[][] { new int[] { 1, 2, 10 }, new int[] { 2, 3, 20 }, new int[] { 2, 5, 25 } };
        var n = 5;
        int[] r = CorpFlightBookings(bookings, n);
        Console.WriteLine(string.Join(",", r));
    }

    private static int[] CorpFlightBookings(int[][] bookings, int n)
    {
        var res = new int[n];
        var diff = new Difference(res);
        foreach (var arr in bookings)
        {
            diff.Increment(arr[0] - 1, arr[1] - 1, arr[2]);
        }
        return diff.Result();
    }
}