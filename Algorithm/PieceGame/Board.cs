
namespace PieceGame;

public class Board
{
    private readonly int _size = 8;
    private readonly Dictionary<Position, IGamePiece> _positionToPiece = new();

    public bool IsWithinBounds(Position pos) => pos.X >= 1 && pos.X <= _size && pos.Y >= 1 && pos.Y <= _size;

    public bool IsOccupied(Position pos) => _positionToPiece.ContainsKey(pos);

    public void PlacePiece(IGamePiece piece)
    {
        if (!IsWithinBounds(piece.CurrentPosition))
            throw new ArgumentException("Invalid position");
        if (IsOccupied(piece.CurrentPosition))
            throw new InvalidOperationException("Position already occupied");
        _positionToPiece[piece.CurrentPosition] = piece;
    }

    public void MovePiece(IGamePiece piece, Position newPos)
    {
        if (!IsWithinBounds(newPos) || IsOccupied(newPos))
            throw new InvalidOperationException("Invalid move");
        _positionToPiece.Remove(piece.CurrentPosition);
        piece.CurrentPosition = newPos;
        _positionToPiece[newPos] = piece;
    }
    
    public IReadOnlyCollection<IGamePiece> GetAllPieces() => _positionToPiece.Values.ToList().AsReadOnly();
}