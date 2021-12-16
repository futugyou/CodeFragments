namespace Labuladong;

public class Code0059
{
    public static void Exection()
    {

    }

    public static int[][] GenerateMatrix(int n)
    {
        int[][] matrix = new int[n][];
        int upper_bound = 0;
        int lower_bound = n - 1;
        int left_bound = 0;
        int right_bound = n - 1;
        int num = 1;
        while (num <= n * n)
        {
            if (upper_bound <= lower_bound)
            {
                for (int i = left_bound; i <= right_bound; i++)
                {
                    matrix[upper_bound][i] = num++;
                }
                upper_bound++;
            }
            if (left_bound <= right_bound)
            {
                for (int i = upper_bound; i <= lower_bound; i++)
                {
                    matrix[i][right_bound] = num++;
                }
                right_bound--;
            }
            if (upper_bound <= lower_bound)
            {
                for (int i = right_bound; i >= left_bound; i--)
                {
                    matrix[lower_bound][i] = num++;
                }
                lower_bound--;
            }
            if (left_bound <= right_bound)
            {
                for (int i = lower_bound; i >= upper_bound; i--)
                {
                    matrix[left_bound][i] = num++;
                }
                left_bound--;
            }
        }
        return matrix;
    }
}