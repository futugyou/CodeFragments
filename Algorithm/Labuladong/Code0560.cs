namespace Labuladong;

public class Code0560
{
    public static void Exection()
    {

    }
    public static int SubarraySum(int[] nums, int k)
    {
        var dic = new Dictionary<int, int>();
        var res = 0;
        var preSum = 0;
        for (int i = 0; i < nums.Length; i++)
        {
            preSum += nums[i];
            int target = preSum - k;
            if (dic.ContainsKey(target))
            {
                res += dic[target];
            }
            if (dic.ContainsKey(preSum))
            {
                dic[preSum] = dic[preSum] + 1;
            }
            else
            {
                dic.Add(preSum, 1);
            }
        }
        return res;
    }
}