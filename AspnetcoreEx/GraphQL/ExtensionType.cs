namespace AspnetcoreEx.GraphQL;
[ExtendObjectType(typeof(User))]
public class UserExtension
{
    [BindMember(nameof(User.Name))]
    public string GetUserName([Parent] User user)
    {
        return user.Name;
    }
}

[ExtendObjectType(typeof(Query))]
public class QueryUserResolvers
{
    public User GetUserByExtendType([Service] IUserRepository repository)
    {
        return repository.GetUserById(1)!;
    }
}