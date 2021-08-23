using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// You have a large array with most of the elements as zero.
    /// Use a more space-efficient data structure, SparseArray, that implements the same interface:
    /// init(arr, size): initialize with the original large array and size.
    /// set(i, val): updates index at i with val.
    /// get(i): gets the value at index i.
    /// </summary>
    public class D00970
    {
        public static void Exection()
        {
            int[] nums = {0,0,0,1,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0 };
            var s = new SparseArray(nums, 3);
            Console.WriteLine(s.Get(3));
            Console.WriteLine(s.Get(14));
            Console.WriteLine(s.Get(2)); 
            s.Set(0, 6);
            s.Set(3, 16);
            Console.WriteLine(s.Get(0));
            Console.WriteLine(s.Get(3));
        }

        private class SparseArray
        {
            private Dictionary<int, int> _dics;
            public SparseArray(int[] nums, int size)
            {
                _dics = new Dictionary<int, int>(size);
                for (int i = 0; i < nums.Length; i++)
                {
                    if (nums[i] != 0)
                    {
                        _dics.Add(i, nums[i]);
                    }
                }
            }

            public void Set(int index, int value)
            {
                if (_dics.ContainsKey(index))
                {
                    _dics[index] = value;
                }
                else
                {
                    _dics.Add(index, value);
                }
            }

            public int Get(int index)
            {
                if (_dics.ContainsKey(index))
                {
                    return _dics[index];
                }
                else
                {
                    return int.MinValue;
                }
            }
        }
    }
}
