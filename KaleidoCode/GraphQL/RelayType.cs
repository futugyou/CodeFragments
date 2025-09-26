namespace KaleidoCode.GraphQL;

[Node]
public class NodeRefetchable
{
    [ID("Id")]
    public string Id { get; set; }
    public string Name { get; set; }

    // [NodeResolver]
    public static async Task<NodeRefetchable> Get(string id, [Service] NodeRefetchableService service)
    {
        var Node = await service.GetByIdAsync(id);
        return Node;
    }
}

public class NodeRefetchableService
{
    public Task<NodeRefetchable> GetByIdAsync(string id)
    {
        return Task.FromResult(new NodeRefetchable { Id = id, Name = "tom" });
    }
}