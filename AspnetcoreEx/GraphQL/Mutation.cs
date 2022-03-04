namespace AspnetcoreEx.GraphQL;

public record AddUserRequest(int id, string name, int age);
public record AddUserResponse(User use);

public class Mutation
{
    /// mutation {
    ///     addUser(user: { id: 101, name: "two", age: 20 }) {
    ///         use {
    ///             id
    ///             name
    ///             age
    ///         }
    ///     }
    /// }
    [Error(typeof(DuplicateException))]
    public Task<AddUserResponse> AddUser(AddUserRequest user, [Service] IUserRepository repository)
    {
        var raw = repository.GetUserById(user.id);
        if (raw != null)
        {
            throw new DuplicateException(user.id);
        }
        User u = new User
        {
            Id = user.id,
            Name = user.name,
            Age = user.age,
        };
        var uselist = repository.AddUser(u);
        return Task.FromResult(new AddUserResponse(uselist.FirstOrDefault(o => o.Id == user.id)!));
    }
}