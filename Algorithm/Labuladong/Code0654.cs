
namespace Labuladong;
public class Code0654
{
    public static void Exection()
    {
        var nums = new int[] { 3, 2, 1, 6, 0, 5 };
        BaseTree tree = Builder(nums, 0, nums.Length - 1);

    }

    private static BaseTree Builder(int[] nums, int left, int right)
    {
        if (left > right)
        {
            return null;
        }
        var maxvalue = int.MinValue;
        var maxindex = -1;
        for (int i = left; i <= right; i++)
        {
            if (nums[i] > maxvalue)
            {
                maxvalue = nums[i];
                maxindex = i;
            }
        }
        BaseTree root = new BaseTree(maxvalue);
        root.Left = Builder(nums, left, maxindex - 1);
        root.Right = Builder(nums, maxindex + 1, right);
        return root;
    }
}