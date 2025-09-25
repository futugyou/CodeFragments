
namespace KaleidoCode.GraphQL.Users;

// A new queryable object named `userName` will be added to `user`
[ExtendObjectType(typeof(User))]
public class UserExtension
{
    // If add this attribute, `name` will be replaced by `username`. 
    // If do not add it, both will coexist.
    [BindMember(nameof(User.Name))]
    public string GetUserName([Parent] User user)
    {
        return user.Name;
    }


    // query{
    //     userByExtendType{
    //       id
    //       userName
    //     }
    // } 
    public User GetUserByExtendType([Service] IUserRepository repository)
    {
        return repository.GetUserById(1)!;
    }
}
 