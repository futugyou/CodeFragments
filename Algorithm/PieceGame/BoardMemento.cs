
namespace PieceGame;

public class BoardMemento
{
    internal Dictionary<Position, IGamePiece> Snapshot { get; }

    public BoardMemento(Dictionary<Position, IGamePiece> snapshot)
    {
        Snapshot = snapshot.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Clone()
        );
    }
}
