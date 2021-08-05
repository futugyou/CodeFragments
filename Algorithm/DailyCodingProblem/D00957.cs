using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// The skyline of a city is composed of several buildings of various widths and heights, 
    /// possibly overlapping one another when viewed from a distance. 
    /// We can represent the buildings using an array of (left, right, height) tuples, 
    /// which tell us where on an imaginary x-axis a building begins and ends, and how tall it is. 
    /// The skyline itself can be described by a list of (x, height) tuples, 
    /// giving the locations at which the height visible to a distant observer changes, and each new height.
    /// Given an array of buildings as described above, create a function that returns the skyline.
    /// For example, suppose the input consists of the buildings[(0, 15, 3), (4, 11, 5), (19, 23, 4)]. 
    /// In aggregate, these buildings would create a skyline that looks like the one below. 
    /// 
    ///      ______  
    ///     |      |        ___
    ///  ___|      |___    |   | 
    /// |   |   B  |   |   | C |
    /// | A |      | A |   |   |
    /// |   |      |   |   |   |
    /// ------------------------
    /// 
    /// As a result, your function should return [(0, 3), (4, 5), (11, 3), (15, 0), (19, 4), (23, 0)].
    /// </summary>
    public class D00957
    {
        public static void Exection()
        {
            var nums = new (int left, int right, int height)[] { (0, 15, 3), (4, 11, 5), (19, 23, 4), (3, 11, 2) };
            int[] maxheight = MakeMaxHeight(nums);
            var result = new List<(int x, int height)>();
            int pre = int.MinValue;
            for (int i = 0; i < maxheight.Length; i++)
            {
                if (pre > maxheight[i])
                {
                    result.Add((i - 1, maxheight[i]));
                    pre = maxheight[i];
                }
                else if (pre < maxheight[i])
                {
                    result.Add((i, maxheight[i]));
                    pre = maxheight[i];
                }
            }
            foreach (var item in result)
            {
                Console.WriteLine($"({item.x},{item.height})");
            }
        }

        private static int[] MakeMaxHeight((int left, int right, int height)[] nums)
        {
            var maxright = nums.Max(p => p.right);
            var result = new int[maxright + 2];
            for (int i = 0; i <= maxright; i++)
            {
                var matches = nums.Where(p => p.left <= i && i <= p.right);
                if (matches.Any())
                {
                    result[i] = matches.Max(p => p.height);
                }
            }
            return result;
        }
    }
}
