namespace AspnetcoreEx.GraphQL;

public class Query
{

    public async Task<User?> GetUser(int id, [Service] IUserRepository repository)
    {
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