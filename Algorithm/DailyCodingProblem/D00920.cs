using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Implement a data structure which carries out the following operations without resizing the underlying array:
    /// add(value) : Add a value to the set of values.
    /// check(value): Check whether a value is in the set.
    /// The check method may return occasional false positives (in other words, incorrectly identifying an element as part of the set), 
    /// but should always correctly identify a true element.
    /// </summary>
    public class D00920
    {
        public static void Exection()
        {
            Bloom bloom = new Bloom(20000);
            bloom.Add("abcd");
            bloom.Add("bcdef");
            var a = bloom.Check("abcd");
            var b = bloom.Check("bcdef");
            var c = bloom.Check("aaaaa");
            Console.WriteLine(a + " " + b + " " + c);
        }
    }

    public class Bloom
    {
        private int[] data;
        private int lenght;
        public Bloom(int num)
        {
            data = new int[(num - 1 + (1 << 5)) >> 5];
            lenght = num;
        }

        public void Add(string input)
        {
            var hash1 = Math.Abs(input.GetHashCode() % lenght);
            var hash2 = GetHashOne(input) % lenght;
            int bitMask = 1 << hash1;
            ref int segment = ref data[hash1 >> 5];
            segment |= bitMask;
            bitMask = 1 << hash2;
            segment = ref data[hash2 >> 5];
            segment |= bitMask;
        }

        public bool Check(string input)
        {
            var hash1 = Math.Abs(input.GetHashCode() % lenght);
            var hash2 = GetHashOne(input) % lenght;
            return (data[hash1 >> 5] & (1 << hash1)) != 0 && (data[hash2 >> 5] & (1 << hash2)) != 0;
        }

        /// <summary>
        /// http://burtleburtle.net/bob/hash/doobs.html
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int GetHashOne(string input)
        {
            int hash = 0;

            for (int i = 0; i < input.Length; i++)
            {
                hash += input[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }
    }
}
