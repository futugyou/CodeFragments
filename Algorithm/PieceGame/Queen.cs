
namespace PieceGame;

public class Queen : IGamePiece
{
    public string Name => "Queen";
    
    public Position CurrentPosition { get; set; }

    public Queen(Position pos)
    {
        CurrentPosition = pos;
    }

    public IEnumerable<Position> GetValidMoves(Board board)
    {
        return GetMoves(board, new[]
        {
            (1,0), (-1,0), (0,1), (0,-1),
            (1,1), (-1,-1), (1,-1), (-1,1),
        });
    }

    public IGamePiece Clone()
    {
        return new Queen(new Position(CurrentPosition.X, CurrentPosition.Y));
    }

    private IEnumerable<Position> GetMoves(Board board, (int dx, int dy)[] directions)
    {
        foreach (var (dx, dy) in directions)
        {
            int x = CurrentPosition.X, y = CurrentPosition.Y;
            while (true)
            {
                x += dx;
                y += dy;
                var pos = new Position(x, y);
                if (!board.IsWithinBounds(pos)) break;
                if (board.IsOccupied(pos)) break;
                yield return pos;
            }
        }
    }
}
