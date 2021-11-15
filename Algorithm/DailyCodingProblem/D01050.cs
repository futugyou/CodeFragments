namespace DailyCodingProblem;

/// <summary>
/// A permutation can be specified by an array P, 
/// where P[i] represents the location of the element at i in the permutation.
/// For example, [2, 1, 0] represents the permutation where elements at the index 0 and 2 are swapped.
/// Given an array and a permutation, apply the permutation to the array.
/// For example, given the array["a", "b", "c"] and the permutation[2, 1, 0], return ["c", "b", "a"].
/// </summary>
public class D01050
{
    public static void Exection()
    {
        var nums = new string[] { "a", "b", "c" };
        var result = new string[nums.Length];
        var order = new int[] { 2, 1, 0 };
        for (int i = 0; i < order.Length; i++)
        {
            var index = order[i];
            result[i] = nums[index];
        }
        Console.WriteLine(string.Join(",", result));
    }
}