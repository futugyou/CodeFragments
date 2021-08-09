using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Other
{
    public class HeapSort
    {
        public static void Sort()
        {
            int[] nums = { 2, 6, 7, 4, 3, 5, 7, 0 };
            int n = nums.Length;
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                BuildHeap(nums, i, n);
            }
            for (int i = nums.Length - 1; i >= 0; i--)
            {
                Swap(nums, 0, i);
                BuildHeap(nums, 0, i);
            }
            foreach (var num in nums)
            {
                Console.WriteLine(num);
            }
        }

        private static void BuildHeap(int[] nums, int index, int length)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            if (left >= length) // that means index is leaf.
            {
                return;
            }
            int maxleaf = left;// max index in 'index's leaf
            if (right < length && nums[right] > nums[left])
            {
                maxleaf = right;
            }
            if (nums[index] < nums[maxleaf])
            {
                Swap(nums, index, maxleaf);
                BuildHeap(nums, maxleaf, length);
            }
        }

        private static void Swap(int[] nums, int left, int right)
        {
            int t = nums[left];
            nums[left] = nums[right];
            nums[right] = t;
        }
    }
}
