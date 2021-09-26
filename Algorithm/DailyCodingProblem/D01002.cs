using System;
namespace DailyCodingProblem
{
    ///  <summary>
    /// Given a sorted array, find the smallest positive integer that is not the sum of a subset of the array.
    /// For example, for the input [1, 2, 3, 10], you should return 7.
    /// Do this in O(N) time.
    ///  </summary>
    public class D01002
    {
        public static void Exection()
        {
            var nums = new int[]{1,2,3,12};
            var sum = 0;
            for (int i = 0; i < nums.Length; i++)
            {
                sum+=nums[i];
                if (i==nums.Length-1)
                {
                    Console.WriteLine(sum+1);  
                }
                else
                {
                    if (sum+1<nums[i+1])
                    {
                         Console.WriteLine(sum+1);
                         break;
                    }
                }
            }
        }
    }
}