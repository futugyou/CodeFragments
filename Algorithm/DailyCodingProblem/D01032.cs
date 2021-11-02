namespace DailyCodingProblem;

/// <summary>
/// In linear algebra, 
/// a Toeplitz matrix is one in which the elements on any given diagonal 
/// from top left to bottom right are identical.
/// Here is an example:
/// 1 2 3 4 8
/// 5 1 2 3 4
/// 4 5 1 2 3
/// 7 4 5 1 2
/// Write a program to determine whether a given input is a Toeplitz matrix.                    |
/// </summary>
public class D01032
{
    public static void Exection()
    {
        var nums = new int[][]{
            new int[]{1, 2, 3, 4, 8 },
            new int[]{5, 1, 2, 3, 4 },
            new int[]{4, 5, 1, 2, 3 },
            new int[]{7, 4, 5, 1, 3 },
        };
        int m = nums.Length;
        int n = nums[0].Length;
        bool IsToeplitz = true;
        for (int i = 0; i < n; i++)
        {
            if (!Check(nums, 0, i))
            {
                IsToeplitz = false;
            }
        }
        if (IsToeplitz)
        {
            for (int i = 0; i < m; i++)
            {
                if (!Check(nums, i, 0))
                {
                    IsToeplitz = false;
                }
            }
        }
        Console.WriteLine(IsToeplitz);
    }
    public static bool Check(int[][] nums, int r, int c)
    {
        int m = nums.Length;
        int n = nums[0].Length;
        var cur = nums[r][c];
        r++;
        c++;
        while (r < m && c < n)
        {
            if (cur != nums[r][c])
            {
                return false;
            }
            r++;
            c++;
        }
        return true;
    }
}