using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFragments
{
    public class NewLinqApiDemo
    {
        public static void Test()
        {
            var raw = Enumerable.Range(1, 10);
            Console.WriteLine(raw.ElementAt(^2)); //9
            Console.WriteLine(string.Join(",", raw.Take(..3))); //1,2,3
            Console.WriteLine(string.Join(",", raw.Take(3..))); //4,5,6,7,8,9,10
            Console.WriteLine(string.Join(",", raw.Take(2..7))); //3,4,5,6,7
            Console.WriteLine(string.Join(",", raw.Take(^3..))); //8,9,10
            Console.WriteLine(string.Join(",", raw.Take(..^3))); //1,2,3,4,5,6,7
            Console.WriteLine(string.Join(",", raw.Take(^7..^2))); //4,5,6,7,8

            Console.WriteLine(string.Join(",", raw.DistinctBy(x => x % 3))); //1,2,3

            var first = new (string Name, int Age)[] { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40) };
            var second = new (string Name, int Age)[] { ("Claire", 30), ("Pat", 30), ("Drew", 33) };
            foreach (var item in first.UnionBy(second, person => person.Age))
            {
                Console.WriteLine(item.Name + "   " + item.Age);
            }    // { ("Francis", 20), ("Lindsey", 30), ("Ashley", 40), ("Drew", 33) }
            foreach (var item in second.UnionBy(first, person => person.Age))
            {
                Console.WriteLine(item.Name + "   " + item.Age);
            }

            var chunks = Enumerable.Range(0, 10).Chunk(size: 3); // { {0,1,2}, {3,4,5}, {6,7,8}, {9} }


            var xs = Enumerable.Range(1, 10);
            var ys = xs.Select(x => x.ToString());
            var zs = xs.Select(x => x % 2 == 0);

            //1   1   False
            //2   2   True
            //3   3   False
            //4   4   True
            //5   5   False
            //6   6   True
            //7   7   False
            //8   8   True
            //9   9   False
            //10   10   True
            foreach ((int x, string y, bool z) in Enumerable.Zip(xs, ys, zs))
            {
                Console.WriteLine(x + "   " + y + "   " + z);
            }
        }
    }
}
