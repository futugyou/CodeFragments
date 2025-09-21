
namespace KaleidoCode.GraphQL;

public class GraphQLOptions
{
    public string SecurityKey { get; set; }
    public string ValidIssuer { get; set; }
    public string ValidAudience { get; set; }
    public string DevPattern { get; set; } = "Code";
}
