namespace Labuladong;
public class Code1631
{
    public static void Exection()
    {
    }

    public static int MinimumEffortPath(int[][] heights)
    {
        int m = heights.Length;
        int n = heights[0].Length;
        int[][] effortTo = new int[m][];
        for (int i = 0; i < m; i++)
        {
            Array.Fill(effortTo[i], int.MaxValue);
        }
        effortTo[0][0] = 0;
        var pq = new PriorityQueue<State, int>();
        pq.Enqueue(new State(0, 0, 0), 0);

        while (pq.Count > 0)
        {
            var curState = pq.Dequeue();
            int curX = curState.X;
            int curY = curState.Y;
            int e = curState.EffortFromStart;
            if (curX == m - 1 && curY == n - 1)
            {
                return e;
            }
            if (e > effortTo[curX][curY])
            {
                continue;
            }
            foreach (var item in Adj(heights, curX, curY))
            {
                int nextX = item[0];
                int nextY = item[1];
                int nextE = Math.Max(effortTo[curX][curY], Math.Abs(heights[curX][curY] - heights[nextX][nextY]));

                if (effortTo[nextX][nextY] > nextE)
                {
                    effortTo[nextX][nextY] = nextE;
                    pq.Enqueue(new State(nextX, nextY, nextE), nextE);
                }
            }
        }
        return -1;
    }
    private static int[][] Dirs = new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
    public static List<int[]> Adj(int[][] matrix, int x, int y)
    {
        int m = matrix.Length;
        int n = matrix[0].Length;
        var neighbors = new List<int[]>();
        foreach (var item in Dirs)
        {
            int nx = x + item[0];
            int ny = y + item[1];
            if (nx >= m || nx < 0 || ny >= n || ny < 0)
            {
                continue;
            }
            neighbors.Add(new int[] { nx, ny });
        }
        return neighbors;
    }

    private class State
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int EffortFromStart { get; set; }
        public State(int x, int y, int e)
        {
            X = x;
            Y = y;
            EffortFromStart = e;
        }
    }
}