namespace Labuladong;
public class Code0130
{
    public static void Exection()
    {
        int[,] grid =  {
            {1, 1, 0, 1, 0 },
            {1, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {1, 1, 0, 1, 1 },
            };
        int m = grid.GetLength(0);
        int n = grid.GetLength(1);
        // "+1" 表示给 dummy 留一个额外位置
        var uf = new UnionFind(m * n + 1);
        int dummy = m * n;// this is "m*n+1"
        // 将首列和末列的 O 与 dummy 连通
        for (int i = 0; i < m; i++)
        {
            if (grid[i, 0] == 0)
            {
                uf.Union(dummy, i * n);
            }
            if (grid[i, n - 1] == 0)
            {
                uf.Union(dummy, i * n + (n - 1));
            }
        }
        // 将首行和末行的 O 与 dummy 连通
        for (int i = 0; i < n; i++)
        {
            if (grid[0, i] == 0)
            {
                uf.Union(dummy, i);
            }
            if (grid[m - 1, i] == 0)
            {
                uf.Union(dummy, n * (m - 1) + i);
            }
        }
        // 方向数组 d 是上下左右搜索的常用手法
        int[,] d = new int[,] { { 1, 0 }, { 0, 1 }, { 0, -1 }, { -1, 0 } };
        for (int i = 1; i < m - 1; i++)
        {
            for (int j = 1; j < n - 1; j++)
            {
                if (grid[i, j] == 0)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        int x = i + d[k, 0];
                        int y = j + d[k, 1];
                        if (grid[x, y] == 0)
                        {
                            uf.Union(x * n + y, i * n + j);
                        }
                    }
                }
            }
        }
        for (int i = 1; i < m - 1; i++)
        {
            for (int j = 1; j < n - 1; j++)
            {
                if (!uf.Connected(dummy, i * n + 1))
                {
                    grid[i, j] = 1;
                }
            }
        }
    }
}