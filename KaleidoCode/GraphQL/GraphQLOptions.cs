
namespace KaleidoCode.GraphQL;

public class GraphQLOptions
{
    public string DevPattern { get; set; } = "Code";
    public bool UseGlobalFilterConvention { get; set; } = false;
    public bool UseGlobalSortConvention { get; set; } = false;
    public bool UseNetInterceptor { get; set; } = true;
    public string PersistedOperations { get; set; } = "InMemory";

}
