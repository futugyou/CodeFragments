namespace Labuladong;

public class Code0215
{
    public static void Exection()
    {
        var nums = new int[] { 3, 2, 3, 1, 2, 4, 5, 5, 6 };
        var k = 4;
        var res = Priority(nums, k);
        Console.WriteLine(res);
        res = FindKthLargest(nums, k);
        Console.WriteLine(res);
    }
    public static int Priority(int[] nums, int k)
    {
        var pq = new PriorityQueue<int, int>();
        foreach (var n in nums)
        {
            pq.Enqueue(n, n);
            if (pq.Count > k)
            {
                pq.Dequeue();
            }
        }
        return pq.Peek();
    }

    public static int FindKthLargest(int[] nums, int k)
    {
        var left = 0;
        var right = nums.Length - 1;
        k = nums.Length - k;
        while (left <= right)
        {
            int p = Partition(nums, left, right);
            if (p < k)
            {
                left = left + 1;
            }
            else if (p > k)
            {
                right = right - 1;
            }
            else
            {
                return nums[p];
            }
        }
        return -1;
    }

    private static int Partition(int[] nums, int left, int right)
    {
        var p = nums[left];
        var i = left;
        var j = right + 1;
        while (true)
        {
            while (nums[++i] < p)
            {
                if (i == right)
                {
                    break;
                }
            }
            while (nums[--j] > p)
            {
                if (j == left)
                {
                    break;
                }
            }
            if (i >= j)
            {
                break;
            }
            var t = nums[i];
            nums[i] = nums[j];
            nums[j] = t;
        }
        nums[left] = nums[j];
        nums[j] = p;
        return j;
    }
}