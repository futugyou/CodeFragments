using HotChocolate.Types.Pagination;

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

    /// DESC/ASC MUST BE UPPER ! UsePaging > UseProjections > UseFiltering > UseSorting
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

    // query {
    //   allUserWithCostomerFilter(where: { name: { eq: "tom" } }, order: { name: ASC }) {
    //     userName
    //   }
    // }
    [UseFiltering(typeof(UserFilterType))]
    [UseSorting(typeof(UserSortType))]
    public Task<List<User>> GetAllUserWithCostomerFilter([Service] IUserRepository repository) => Task.FromResult(repository.GetAllUser());

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

    // query {
    //   customPagingUser(after: "1", first: 2, sortBy: "id")
    //     {
    //         pageInfo {
    //             hasNextPage
    //             hasPreviousPage
    //         }
    //         edges {
    //             cursor
    //             node {
    //                 id
    //                 age
    //             }
    //         }
    //         nodes {
    //             id
    //             userName
    //         }
    //     }
    // }
    [UsePaging]
    public Task<Connection<User>> CustomPagingUser(string? after, int? first, string sortBy, [Service] IUserRepository repository)
    {
        int.TryParse(after, out var afrinint);
        IEnumerable<User> users = repository.GetAllUser().Skip(afrinint).Take(first == null ? 0 : first.Value);

        var edges = users.Select(user => new Edge<User>(user, user.Id.ToString()))
                            .ToList();
        var pageInfo = new ConnectionPageInfo(false, false, null, null);

        var connection = new Connection<User>(edges, pageInfo,
                            ct => ValueTask.FromResult(0));

        return Task.FromResult(connection);
    }

}


public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field(f => f.GetPagingUser(default!))
            .Type<ListType<UserType>>();
        descriptor
            .Field(f => f.GetTakeSkipUser(default!))
            .Type<ListType<UserType>>();
        descriptor
            .Field(f => f.GetAllUser(default!))
            //.UseConsoleLogMiddleware()
            .Use<CustomLogMiddleware>()
            .Type<ListType<UserType>>();
    }
}