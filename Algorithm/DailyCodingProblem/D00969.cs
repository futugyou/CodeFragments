using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Given a matrix of 1s and 0s, return the number of "islands" in the matrix. 
    /// A 1 represents land and 0 represents water, so an island is a group of 1s that are neighboring whose perimeter is surrounded by water.

    ///   For example, this matrix has 4 islands.

    /// 1 0 0 0 0
    /// 0 0 1 1 0
    /// 0 1 1 0 0
    /// 0 0 0 0 0
    /// 1 1 0 0 1
    /// 1 1 0 0 1
    /// </summary>
    public class D00969
    {
        public static void Exection()
        {
            int[,] nums = {
                {1, 0 ,0 ,0 ,0},
                {0, 0, 1, 1, 0},
                {0 ,1, 1, 0, 0},
                {0, 0, 0, 0, 0},
                {1 ,1 ,0 ,0 ,1},
                {1, 1, 0, 0 ,1}
            };
            var row = nums.GetLength(0);
            var col = nums.GetLength(1);
            int count = 0;
            var visited = new List<(int, int)>();

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (nums[i, j] == 1)
                    {
                        if (visited.Contains((i, j)))
                        {
                            continue;
                        }
                        count++;
                        var queue = new Queue<(int, int)>();
                        queue.Enqueue((i, j));
                        while (queue.Any())
                        {
                            var (ii, jj) = queue.Dequeue();
                            if (visited.Contains((ii, jj)))
                            {
                                continue;
                            }
                            visited.Add((ii, jj));
                            if (ii > 0 && nums[ii - 1, jj] == 1)
                            {
                                queue.Enqueue((ii - 1, jj));
                            }

                            if (ii < row - 1 && nums[ii + 1, jj] == 1)
                            {
                                queue.Enqueue((ii + 1, jj));
                            }
                            if (jj > 0 && nums[ii, jj - 1] == 1)
                            {
                                queue.Enqueue((ii, jj - 1));
                            }
                            if (jj < col - 1 && nums[ii, jj + 1] == 1)
                            {
                                queue.Enqueue((ii, jj + 1));
                            }
                        }
                    }
                }
            }

            Console.WriteLine(count);
        }
    }
}
