namespace Labuladong;

public class Code0048
{
    public static void Exection()
    {

    }

    public static void Rotate(int[][] matrix)
    {
        var n = matrix.Length;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                int t = matrix[i][j];
                matrix[i][j] = matrix[j][i];
                matrix[j][i] = t;
            }
        }

        for (int i = 0; i < n; i++)
        {
            var row = matrix[i];
            int left = 0;
            int right = row.Length - 1;
            while (left < right)
            {
                var t = row[left];
                row[left] = row[right];
                row[right] = t;
                left++;
                right--;
            }
        }
    }
}