namespace Labuladong;

public class Code0303
{
    public static void Exection()
    {

    }

    public class NumArray
    {
        private int[] preNum;
        public NumArray(int[] p)
        {
            preNum = new int[p.Length + 1];
            for (int i = 1; i < preNum.Length; i++)
            {
                preNum[i] = preNum[i - 1] + p[i - 1];
            }
        }

        public int SumRange(int start, int end)
        {
            return preNum[end + 1] - preNum[start];
        }
    }
}