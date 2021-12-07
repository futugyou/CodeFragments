namespace Labuladong;

public class Code0037
{

    public static void Exection()
    {

    }

    public static bool Backtrack(char[,] board, int i, int j)
    {
        int m = 9;
        int n = 9;
        if (j == n) return Backtrack(board, i + 1, 0);
        if (i == m) return true;
        if (board[i, j] != '.') return Backtrack(board, i, j + 1);
        for (char ch = '1'; ch <= '9'; ch++)
        {
            if (!IsValid(board, i, j, ch))
            {
                continue;
            }
            board[i, j] = ch;
            if (Backtrack(board, i, j + 1))
            {
                return true;
            }
            board[i, j] = '.';
        }
        return false;
    }

    public static bool IsValid(char[,] board, int r, int c, char n)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[r, i] == n) return false;
            if (board[i, c] == n) return false;
            if (board[(r / 3) * 3 + i / 3, (c / 3) * 3 + i % 3] == n) return false;
        }
        return true;
    }

}