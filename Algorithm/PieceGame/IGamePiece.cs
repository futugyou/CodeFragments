namespace PieceGame;

public interface IGamePiece
{
    string Name { get; }
    Position CurrentPosition { get; set; }
    IEnumerable<Position> GetValidMoves(Board board);
}