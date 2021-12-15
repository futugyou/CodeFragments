namespace Labuladong;

public class Code0054
{
    public static void Exection()
    {

    }

    public static List<int> SpiralOrder(int[][] matrix)
    {
        int m = matrix.Length;
        int n = matrix[0].Length;
        int upper_bound = 0;
        int left_bound = 0;
        int lower_bound = m - 1;
        int right_bound = n - 1;
        var res = new List<int>();
        while (res.Count < m * n)
        {
            if (upper_bound <= lower_bound)
            {
                for (int j = left_bound; j <= right_bound; j++)
                {
                    res.Add(matrix[upper_bound][j]);
                }
                upper_bound++;
            }

            if (left_bound <= right_bound)
            {
                for (int j = upper_bound; j < lower_bound; j++)
                {
                    res.Add(matrix[j][right_bound]);
                }
                right_bound--;
            }
            if (upper_bound <= lower_bound)
            {
                for (int j = right_bound; j >= left_bound; j--)
                {
                    res.Add(matrix[lower_bound][j]);
                }
                lower_bound--;
            }
            if (left_bound <= right_bound)
            {
                for (int j = lower_bound; j >= upper_bound; j--)
                {
                    res.Add(matrix[j][left_bound]);
                }
            }
        }
        return res;
    }
}