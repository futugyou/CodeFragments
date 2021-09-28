namespace DailyCodingProblem
{
    /// <summary>
    /// Given a list of numbers L, 
    /// implement a method sum(i, j) which returns the sum from the sublist L[i:j] (including i, excluding j).
    /// For example, given L = [1, 2, 3, 4, 5], sum(1, 3) should return sum([2, 3]), which is 5.
    /// You can assume that you can do some pre-processing. sum() should be optimized over the pre-processing step.
    /// </summary>
    public class D01004
    {
        public static void Exection()
        {
            var nums = new int[]{1, 2, 3, 4, 5};
            var tmp = new int[nums.Length];
            tmp[0] = nums[0];
            for (int i = 1; i < nums.Length; i++)
            {
                tmp[i] = nums[i] + tmp[i-1];
            }
            // sum(1,3)
            Console.WriteLine(tmp[2]-tmp[0]);
            // sum(0,3)
            Console.WriteLine(tmp[2]-0);
            // sum(1,4)
            Console.WriteLine(tmp[3]-tmp[0]);
            // sum(1,5)
            Console.WriteLine(tmp[4]-tmp[0]);
        }
    }
}