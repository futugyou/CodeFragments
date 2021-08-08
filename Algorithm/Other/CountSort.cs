using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Other
{
    public class CountSort
    {
        public static void Sort()
        {
            int[] nums = { 9, 8, 7, 5, 6, 4, 2, 3, 1 };
            int max = nums.Max();
            var helper = new int[max + 1];
            foreach (var no in nums)
            {
                helper[no]++;
            }
            int index = 0;
            for (int i = 1; i < helper.Length; i++)
            {
                int item = helper[i];
                for (int j = 0; j < item; j++)
                {
                    nums[index] = i;
                    index++;
                }
            }
            for (int i = 0; i < nums.Length; i++)
            {
                Console.WriteLine(nums[i]);
            }
        }
    }
}
