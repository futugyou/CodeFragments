namespace PieceGame;

public class SimpleGame : IGame
{
    private readonly Random _rnd = new Random();

    private Position _startPosition;

    public void Play(int moves)
    {
        var knight = new KnightMove();
        var pos = _startPosition;
        Console.WriteLine("0: My position is {0}", pos);
        for(var move = 1; move <= moves; move++)
        {
            var possibleMoves = knight.ValidMovesFor(pos).ToArray();
            pos = possibleMoves[_rnd.Next(possibleMoves.Length)];
            Console.WriteLine("{1}: My position is {0}", pos, move);
        }
    }
    
    public void Setup()
    {
        _startPosition = new Position(3, 3);
    }
}