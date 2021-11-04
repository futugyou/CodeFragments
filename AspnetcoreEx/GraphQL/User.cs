namespace AspnetcoreEx.GraphQL;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Query
{
    public async Task<User> GetUser(int id)
    {
        return await Task.FromResult(new User { Id = id, Name = "Name_" + id, Age = new Random().Next(20, 30) });
    }
}