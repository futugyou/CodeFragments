using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a N by M matrix of numbers, print out the matrix in a clockwise spiral.
    /// For example, given the following matrix:
    /// [[1,  2,  3,  4,  5],
    /// [6,  7,  8,  9,  10],
    /// [11, 12, 13, 14, 15],
    /// [16, 17, 18, 19, 20]]
    /// You should print out the following:
    /// 1
    /// 2
    /// 3
    /// 4
    /// 5
    /// ...
    /// </summary>
    public class D00939
    {
        public static void Exection()
        {
            int[,] nums =  {
            {1,  2,  3,  4,  5 },
            {6,  7,  8,  9,  10 },
            {11, 12, 13, 14, 15 },
            {16, 17, 18, 19, 20 },
            {21, 22, 23, 24, 25 },
            {26, 27, 28, 29, 30 },
            {31, 32, 33, 34, 35 },
            };
            var count = nums.Length;
            var horizontal = nums.GetUpperBound(1);
            var vertical = nums.GetUpperBound(0);
            var hrcount = 0;
            var hlcount = 0;
            var vucount = 1;
            var vdcount = 0;
            var h = 0;
            var hstep = 1;
            var v = 0;
            var vstep = 1;
            var hv = true;
            for (int i = 0; i < count; i++)
            {
                if (hv)
                {
                    if (hstep == 1)
                    {
                        Console.WriteLine(nums[v, h]);
                        if (h < horizontal - hrcount)
                        {
                            h++;
                        }
                        else
                        {
                            hrcount++;
                            v++;
                            hstep = -1;
                            vstep = 1;
                            hv = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine(nums[v, h]);
                        if (h > hlcount)
                        {
                            h--;
                        }
                        else
                        {
                            hlcount++;
                            v--;
                            hstep = 1;
                            vstep = -1;
                            hv = false;
                        }
                    }
                }
                else
                {
                    if (vstep == 1)
                    {
                        Console.WriteLine(nums[v, h]);
                        if (v < vertical - vdcount)
                        {
                            v++;
                        }
                        else
                        {
                            h--;
                            vdcount++;
                            vstep = -1;
                            hstep = -1;
                            hv = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine(nums[v, h]);
                        if (v > vucount)
                        {
                            v--;
                        }
                        else
                        {
                            h++;
                            vucount++;
                            vstep = 1;
                            hstep = 1;
                            hv = true;
                        }
                    }
                }
            }
        }
    }
}
