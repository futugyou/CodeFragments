namespace AspnetcoreEx.GraphQL;
using HotChocolate.Subscriptions;

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
    public async Task<AddUserResponse> AddUser(AddUserRequest user, [Service] IUserRepository repository, [Service] ITopicEventSender sender)
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
        Console.WriteLine("send user create message, user id: " + u.Id);
        await sender.SendAsync("userCreated", u);
        return new AddUserResponse(uselist.FirstOrDefault(o => o.Id == user.id)!);
    }

    public IPet? CreatePet(PetInput input)
    {
        if (input.Cat != null)
        {
            return input.Cat;
        }
        return input.Dog;
    }
}

public class MutationType : ObjectType<Mutation>
{
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor)
    {
        descriptor
        .Field(f => f.AddUser(default!, default!, default!))
        .Argument("user", a => a.Description("this is user input parameter description!"))
        //.Error<DuplicateException>()
        .UseMutationConvention();
    }
}