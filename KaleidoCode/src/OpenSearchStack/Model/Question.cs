
namespace OpenSearchStack.Model;

public class Question
{
    public int Id { get; set; }
    public DateTimeOffset CreationDate { get; set; }
    public int Score { get; set; }
    public string Body { get; set; }
}