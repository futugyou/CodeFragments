using System;
namespace DailyCodingProblem
{
    /// <summary>
    /// Suppose you have a multiplication table that is N by N. 
    /// That is, a 2D array where the value at the i-th row and j-th column is (i + 1) * (j + 1) (if 0-indexed) 
    /// or i * j (if 1-indexed).
    /// Given integers N and X, 
    /// write a function that returns the number of times X appears as a value in an N by N multiplication table.
    /// </summary>
    public class D00999
    {
        public  static void Exection()
        {
            var n = 6;
            var x = 12;
            for (int i = 1; i <= 12; i++)
            {
                if (x % i == 0 && n >= i && n >= (x / i))
                { 
                    Console.WriteLine(i);
                }
            }
        }
    }
}