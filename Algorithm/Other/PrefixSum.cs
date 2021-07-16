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
}
