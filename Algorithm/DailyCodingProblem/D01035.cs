namespace DailyCodingProblem;

/// <summary>
/// MegaCorp wants to give bonuses to its employees based on how many lines of codes they have written. 
/// They would like to give the smallest positive amount to each worker consistent with the constraint 
/// that if a developer has written more lines of code than their neighbor, they should receive more money.

/// Given an array representing a line of seats of employees at MegaCorp, 
/// determine how much each one should get paid.

/// For example, given[10, 40, 200, 1000, 60, 30], you should return [1, 2, 3, 4, 2, 1].
/// </summary>
public class D01035
{
    public static void Exection()
    {
        var nums = new int[] { 10, 40, 200, 1000, 60, 30, 10, 9, 8 };
        var list = new int[nums.Length];
        list[0] = 1;
        var prew = nums[0];
        var index = 0;
        for (int i = 1; i < nums.Length; i++)
        {
            var curr = nums[i];
            if (curr > prew)
            {
                var last = list[i - 1];
                last++;
                list[i] = last;
                index = i;
            }
            else
            {
                list[i] = 1;
                for (int a = i; a > index; a--)
                {
                    if (list[a] < list[a - 1])
                    {
                        continue;
                    }
                    list[a - 1]++;
                }
            }
            prew = curr;
        }

        Console.WriteLine(string.Join(",", list));
    }
}