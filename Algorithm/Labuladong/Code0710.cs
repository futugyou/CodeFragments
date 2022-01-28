namespace Labuladong;
public class Code0710
{
    public static void Exection()
    {
        var blackList = new int[] { 3, 1 };
        var n = 5;
        var rand = new RandomWithBlackList(n, blackList);
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(rand.Pick());
        }
    }

    public class RandomWithBlackList
    {
        private int lenght = 0;
        private Random r;
        private Dictionary<int, int> mappingBlacklistToEndOfArrayIndex = new Dictionary<int, int>();
        public RandomWithBlackList(int n, int[] blackList)
        {
            r = new Random();
            lenght = n - blackList.Length;

            var lastindex = n - 1;
            foreach (var b in blackList)
            {
                mappingBlacklistToEndOfArrayIndex[b] = int.MinValue;
            }
            foreach (var b in blackList)
            {
                if (b >= lenght)
                {
                    continue;
                }
                while (mappingBlacklistToEndOfArrayIndex.ContainsKey(lastindex))
                {
                    lastindex--;
                }
                mappingBlacklistToEndOfArrayIndex[b] = lastindex;
                lastindex--;
            }
        }

        public int Pick()
        {
            var index = r.Next() % lenght;
            if (mappingBlacklistToEndOfArrayIndex.ContainsKey(index))
            {
                return mappingBlacklistToEndOfArrayIndex[index];
            }
            return index;
        }
    }
}