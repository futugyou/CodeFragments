namespace DailyCodingProblem;

/// <summary>
/// On a mysterious island there are creatures known as Quxes which come in three colors: red, green, and blue. 
/// One power of the Qux is that if two of them are standing next to each other, 
/// they can transform into a single creature of the third color.

/// Given N Quxes standing in a line, 
/// determine the smallest number of them remaining after any possible sequence of such transformations.

/// For example, given the input ['R', 'G', 'B', 'G', 'B'], 
/// it is possible to end up with a single Qux through the following steps:

/// Arrangement       |   Change
/// ----------------------------------------
/// ['R', 'G', 'B', 'G', 'B'] | (R, G) -> B
/// ['B', 'B', 'G', 'B']      | (B, G) -> R
/// ['B', 'R', 'B']           | (R, B) -> G
/// ['B', 'G']                | (B, G) -> R
/// ['R']                     |
/// </summary>
public class D01030
{
    public static void Exection()
    {
        var nums = new string[] { "R", "G", "B", "G", "B" };
        List<string> q = new List<string>();
        for (int i = 0; i < nums.Length; i++)
        {
            q.Add(nums[i]);
        }
        var first = q.FirstOrDefault();
        var index = 1;
        while (q.Any())
        {
            var second = q[index];
            if (first.Equals(second))
            {
                index++;
                first = second;
                continue;
            }
            var t = ChangeString(first, second);

            q.RemoveAt(index);
            q.RemoveAt(index - 1);
            q.Insert(index - 1, t);
            first = q.FirstOrDefault();
            index = 1;
            Console.WriteLine(string.Join(",", q));
            if (q.Distinct().Count() == 1)
            {
                break;
            }
        }
    }

    private static string ChangeString(string first, string second)
    {
        if ("R".Equals(first) && "G".Equals(second) || "R".Equals(second) && "G".Equals(first))
        {
            return "B";
        }
        if ("B".Equals(first) && "G".Equals(second) || "B".Equals(second) && "G".Equals(first))
        {
            return "R";
        }
        if ("R".Equals(first) && "B".Equals(second) || "R".Equals(second) && "B".Equals(first))
        {
            return "G";
        }
        return "";
    }
}