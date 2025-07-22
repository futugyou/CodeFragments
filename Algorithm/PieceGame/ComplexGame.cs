namespace PieceGame;

public class ComplexGame : IGame
{
    private readonly Board _board = new();
    private readonly Random _rnd = new();
    public void Setup()
    {
        var pieces = new IGamePiece[]
        {
            new Knight(new Position(2, 1)),
            new Bishop(new Position(4, 4)),
            new Queen(new Position(6, 6)),
            new Knight(new Position(1, 8)),
            new Bishop(new Position(7, 3))
        };
        foreach (var piece in pieces)
        {
            _board.PlacePiece(piece);
        }
    }
    public void Play(int moves)
    {
        var pieces = _board.GetAllPieces().ToList();
        for (int move = 1; move <= moves; move++)
        {
            var piece = pieces[_rnd.Next(pieces.Count)];
            var validMoves = piece.GetValidMoves(_board).ToList();
            if (!validMoves.Any())
            {
                Console.WriteLine($"{move}: {piece.Name} at {piece.CurrentPosition} has no valid moves");
                continue;
            }
            var chosenMove = validMoves[_rnd.Next(validMoves.Count)];
            _board.MovePiece(piece, chosenMove);
            Console.WriteLine($"{move}: {piece.Name} moved to {chosenMove}");
        }
    }
}