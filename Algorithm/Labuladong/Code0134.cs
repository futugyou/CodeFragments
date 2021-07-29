using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labuladong
{
    public class Code0134
    {
        public static int CanCompleteCircuit()
        {
            var gas = new int[] { 1, 2, 3, 4, 5 };
            var cost = new int[] { 3, 4, 5, 1, 2 };
            return Greedy(gas, cost);
            //return Graphics(gas, cost);
            //return ForFor(gas, cost);
        }

        private static int Greedy(int[] gas, int[] cost)
        {
            int sum = 0;
            for (int i = 0; i < gas.Length; i++)
            {
                sum += gas[i] - cost[i];
            }
            if (sum < 0)
            {
                return -1;
            }
            sum = 0;
            int start = 0;
            for (int i = 0; i < gas.Length; i++)
            {
                sum += gas[i] - cost[i];
                if (sum < 0)
                {
                    sum = 0;
                    start = i + 1;
                }
            }
            return start == gas.Length ? 0 : start;
        }

        private static int Graphics(int[] gas, int[] cost)
        {
            int sum = 0;
            int minsum = int.MaxValue;
            int start = 0;
            for (int i = 0; i < gas.Length; i++)
            {
                sum += gas[i] - cost[i];
                if (sum < minsum)
                {
                    start = i + 1;
                    minsum = sum;
                }
            }
            if (sum < 0)
            {
                return -1;
            }
            return start == gas.Length ? 0 : start;
        }

        private static int ForFor(int[] gas, int[] cost)
        {
            //1. for for
            for (int i = 0; i < gas.Length; i++)
            {
                int sum = 0;
                for (int j = 0; j < gas.Length; j++)
                {
                    sum += gas[j] - cost[j];
                    if (sum < 0)
                    {
                        break;
                    }
                }
                if (sum >= 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
