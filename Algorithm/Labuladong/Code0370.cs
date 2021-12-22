namespace Labuladong;

public class Code0370
{
    public static void Exection()
    {
        
    }

    public static int[] GetModifiedArray(int length, int[][] updates)
    {
        var nums = new int[length];
        var diff = new Difference(nums);
        foreach(var item in updates)
        {
           diff. Increment(item[0],item[1],item[2]);
        }
        return diff.Result();
    }

    public class Difference
    {
        private int[] diff;
        
        public Difference(int[] nums)
        {
            diff = new int[nums.Length];
            diff[0] = nums[0];
            for(int i = 1; i<nums.Length;i++)
            {
                diff[i] = nums[i] - nums[i-1];
            }
        }

        public void Increment(int start,int end,int value)
        {
            diff[start]+=value;
            if(end+1<diff.Length)
            {
                diff[end+1] -=value;
            }
        }

        public int[] Result()
        {
            int[] res = new int[diff.Length];
            res[0] = diff[0];
            for(int i =1;i<diff.Length;i++)
            {
                res[i] = res[i-1]+diff[i];
            }
            return res;
        }
    }
}