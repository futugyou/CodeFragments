/*
 * @lc app=leetcode.cn id=739 lang=csharp
 *
 * [739] 每日温度
 */

// @lc code=start
public class Solution
{
    public int[] DailyTemperatures(int[] temperatures)
    {
        var res = new int[temperatures.Length];
        var q = new Stack<int>();
        for (int i = 0; i < temperatures.Length; i++)
        {
            while (q.Count > 0 && temperatures[i] > temperatures[q.Peek()])
            {
                var t = q.Pop();
                res[t] = i - t;
            }
            q.Push(i);
        }
        return res;
    }
}
// @lc code=end

