
using HotChocolate.Subscriptions;

namespace KaleidoCode.GraphQL.Users;

public record AddUserRequest(int id, string name, int age);
public record AddUserResponse(User use);

[ExtendObjectType(typeof(Mutation))]
public class UserMutation
{
    // mutation {
    //     addUser(input: { user: { id: 101, name: "two", age: 20 } }) {
    //         addUserResponse {
    //         use {
    //             id
    //             userName
    //             age
    //         }
    //         }
    //     }
    // }
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
}
