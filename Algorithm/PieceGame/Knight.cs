namespace PieceGame;

public class Knight : IGamePiece
{
    private readonly KnightMove _moveGen = new();

    public string Name => "Knight";

    public Position CurrentPosition { get; set; }

    public Knight(Position pos)
    {
        CurrentPosition = pos;
    }

    public IEnumerable<Position> GetValidMoves(Board board)
    {
        return _moveGen.ValidMovesFor(CurrentPosition).Where(p => !board.IsOccupied(p));
    }

    private class KnightMove
    {
        public static readonly int[,] Moves = new[,] { { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 }, { 2, 1 }, { -2, 1 }, { 2, -1 }, { -2, -1 } };
        public IEnumerable<Position> ValidMovesFor(Position pos)
        {
            for(var i=0;i<=Moves.GetUpperBound(0);i++)
            {
                var newX = pos.X + Moves[i,0];
                var newY = pos.Y + Moves[i,1];
                if (newX > 8 || newX < 1 || newY > 8 || newY < 1)
                    continue;
                yield return new Position(newX, newY);
            }
        }
    }
}
