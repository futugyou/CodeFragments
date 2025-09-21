namespace KaleidoCode.GraphQL;

[Node]
public class UserRefetchable
{
    [ID("Id")]
    public string Id { get; set; }
    public string Name { get; set; }

    // [NodeResolver]
    public static async Task<UserRefetchable> Get(string id, [Service] UserRefetchableService service)
    {
        var user = await service.GetByIdAsync(id);
        return user;
    }
}

public class UserRefetchableService
{
    public Task<UserRefetchable> GetByIdAsync(string id)
    {
        return Task.FromResult(new UserRefetchable { Id = id, Name = "tom" });
    }
}