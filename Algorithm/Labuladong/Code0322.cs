using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labuladong
{
    public class Code0322
    {
        public static int CoinChange(int[] coins, int amount)
        {
            if (amount == 0)
            {
                return -1;
            }
            // dp[i] : when amount = i, mix coin count is dp[i].
            int[] dp = new int[coins.Length + 1];
            for (int i = 0; i < dp.Length; i++)
            {
                dp[i] = -1;
            }
            dp[0] = 0;
            for (int i = 1; i <= amount; i++)
            {
                foreach (var coin in coins)
                {
                    if (i < coin)
                    {
                        continue;
                    }
                    dp[i] = Math.Min(dp[i], dp[i - coin] + 1);
                }
            }
            return dp[amount];
        }
    }
}
