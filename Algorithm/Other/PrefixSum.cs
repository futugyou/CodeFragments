using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Other
{
    public class PrefixSum
    {
        private int[] prefix;
        public PrefixSum(int[] nums)
        {
            prefix = new int[nums.Length + 1];
            for (int i = 1; i < nums.Length; i++)
            {
                prefix[i] = prefix[i - 1] + nums[i];
            }
        }

        /// <summary>
        /// sum [i,j]
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public int QuerySum(int i, int j)
        {
            return prefix[j + 1] - prefix[i];
        }
    }

    public class DiffArray
    {
        private int[] diff;
        public DiffArray(int[] nums)
        {
            diff = new int[nums.Length];
            diff[0] = nums[0];
            for (int i = 1; i < nums.Length; i++)
            {
                diff[i] = nums[i] - nums[i - 1];
            }
        }

        /// <summary>
        /// increment [i,j] value
        /// </summary>
        public void Increment(int i, int j, int value)
        {
            diff[i] += value;
            if (j + i < diff.Length)
            {
                diff[j + 1] -= value;
            }
        }

        public int[] Rebuild()
        {
            var result = new int[diff.Length];
            result[0] = diff[0];
            for (int i = 1; i < diff.Length; i++)
            {
                result[i] = result[i - 1] + diff[i];
            }
            return result;
        }
    }
}
