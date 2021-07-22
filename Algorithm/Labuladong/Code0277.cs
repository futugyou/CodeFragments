using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labuladong
{
    public class Code0277
    {
        public static int FindCelebrity()
        {
            int[,] nums =  {
            {1, 1, 1, 0 },
            {1, 1, 1, 1 },
            {0, 0, 1, 0 },
            {0, 0, 1, 1 },
            };
            // var result = ExecOne(nums);
            var result = ExecTwo(nums);
            Console.WriteLine(result);
            return result;
        }

        private static int ExecTwo(int[,] nums)
        {
            int n = (int)Math.Sqrt(nums.Length);
            int cand = 0;
            // 排除掉非名人
            for (int other = 0; other < n; other++)
            {
                if (cand == other) continue;
                if (nums[cand, other] == 1 || nums[other, cand] == 0)
                {
                    cand = other;
                }
            }
            // 确认cand是否是名人
            for (int other = 0; other < n; other++)
            {
                if (cand == other) continue;
                if (nums[cand, other] == 1 || nums[other, cand] == 0)
                {
                    return -1;
                }
            }
            return cand;
        }

        private static int ExecOne(int[,] nums)
        {
            int n = (int)Math.Sqrt(nums.Length);
            for (int i = 0; i < n; i++)
            {
                bool allknow = false;
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        if (nums[i, j] == 1 || nums[j, i] == 0)
                        {
                            allknow = false;
                            break;
                        }
                    }
                    allknow = true;
                }
                if (allknow)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
