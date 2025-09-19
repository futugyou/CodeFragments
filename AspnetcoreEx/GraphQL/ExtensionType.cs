namespace AspnetcoreEx.GraphQL;

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
}

// query{
//     userByExtendType{
//       id
//       userName
//     }
// } 
// A new queryable object named `userByExtendType` will be added to `query`
[ExtendObjectType(typeof(Query))]
public class QueryUserResolvers
{
    public User GetUserByExtendType([Service] IUserRepository repository)
    {
        return repository.GetUserById(1)!;
    }
}