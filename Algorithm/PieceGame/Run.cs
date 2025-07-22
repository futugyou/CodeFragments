
namespace PieceGame;

public class Run
{
    IGame game = new ComplexGame();
    game.Setup();
    game.Play(15);
}