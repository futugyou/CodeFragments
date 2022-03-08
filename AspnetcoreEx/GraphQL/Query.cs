namespace AspnetcoreEx.GraphQL;

public class Query
{
    // query {
    //   a: userWithDataLoader(id: 1)
    //     {
    //         id
    //     }
    //     b: userWithDataLoader(id: 2)
    //     {
    //         id
    //         userName
    //     }
    // }
    public async Task<User> GetUserWithDataLoader(int id, UserBatchDataLoader dataLoader)
    {
        return await dataLoader.LoadAsync(id);
    }

    public async Task<User?> GetUser(
    int id,
    [Service] IUserRepository repository,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext is not null)
        {
            Console.WriteLine("the getuser mothed path is " + httpContextAccessor.HttpContext.Request.Path);
        }
        var user = repository.GetAllUser().FirstOrDefault(p => p.Id == id);
        if (user == null)
        {
            return null;
        }
        return await Task.FromResult(user);
    }

    /// DESC/ASC MUST BE UPPER
    ///query {
    /// allUser(where: { id: { eq: 2 } }, order: {id: DESC/ASC}) {
    ///     id
    ///     name
    ///     orders {
    ///         orderTime
    ///     }
    /// }
    ///}
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public Task<List<User>> GetAllUser([Service] IUserRepository repository) => Task.FromResult(repository.GetAllUser());

    /// query {
    ///     takeSkipUser(take: 1, skip: 1) {
    ///         pageInfo {
    ///             hasNextPage
    ///             hasPreviousPage
    ///         }
    ///     totalCount
    ///     items {
    ///         id
    ///         name
    ///         age
    ///         }
    ///     }
    /// }
    [UseOffsetPaging]
    public Task<List<User>> GetTakeSkipUser([Service] IUserRepository repository) => Task.FromResult(repository.GetAllUser());

    ///query {
    /// pagingUser(first: 2) {
    ///     pageInfo {
    ///         hasNextPage
    ///         hasPreviousPage
    ///     }
    /// totalCount
    /// edges {
    ///     cursor
    ///     node {
    ///         id
    ///         name
    ///         age
    ///         }
    ///     }
    /// }
    ///}
    [UsePaging]
    public Task<List<User>> GetPagingUser([Service] IUserRepository repository) => Task.FromResult(repository.GetAllUser());
}


public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field(f => f.GetPagingUser(default!))
            .Type<UserType>();
        descriptor
            .Field(f => f.GetTakeSkipUser(default!))
            .Type<UserType>();
    }
}