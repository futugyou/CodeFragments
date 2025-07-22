

namespace PieceGame;

public struct Position
{
    public readonly int X;
    public readonly int Y;

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj is Position)
        {
            var val = (Position) obj;
            return val.X==X && val.Y==Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return X^Y;
    }

    public override string ToString()
    {
        return String.Format("{0},{1}", X, Y);
    }
}