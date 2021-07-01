using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    public class D00919
    {
        /// <summary>
        /// A wall consists of several rows of bricks of various integer lengths and uniform height. 
        /// Your goal is to find a vertical line going from the top to the bottom of the wall that cuts through the fewest number of bricks. 
        /// If the line goes through the edge between two bricks, this does not count as a cut.
        /// For example, suppose the input is as follows, where values in each row represent the lengths of bricks in that row:
        ///      [[3, 5, 1, 1],
        ///       [2, 3, 3, 2],
        ///       [5, 5],
        ///       [4, 4, 2],
        ///       [1, 3, 3, 3],
        ///       [1, 1, 6, 1, 1]]
        /// The best we can we do here is to draw a line after the eighth brick, which will only require cutting through the bricks in the third and fifth row.
        /// Given an input consisting of brick lengths for each row such as the one above, 
        /// return the fewest number of bricks that must be cut to create a vertical line.
        /// </summary>
        public static void Exection()
        {
            int[][] raw = new int[6][];
            raw[0] = new int[4] { 3, 5, 1, 1 };
            raw[1] = new int[4] { 2, 3, 3, 2 };
            raw[2] = new int[2] { 5, 5 };
            raw[3] = new int[3] { 4, 4, 2 };
            raw[4] = new int[4] { 1, 3, 3, 3 };
            raw[5] = new int[5] { 1, 1, 6, 1, 1 };

            int sumlength = raw[0].Sum();
            for (int i = 0; i < raw.Length; i++)
            {
                for (int j = 1; j < raw[i].Length; j++)
                {
                    raw[i][j] = raw[i][j] + raw[i][j - 1];
                }
            }
            var result = raw.SelectMany(p => p.Select(a => a)).Where(p => p != sumlength).GroupBy(p => p).OrderByDescending(p => p.Count()).FirstOrDefault();//.OrderByDescending(p => p.Count()).FirstOrDefault();
            Console.WriteLine(result.Key + "  " + result.Count());
        }
    }
}
