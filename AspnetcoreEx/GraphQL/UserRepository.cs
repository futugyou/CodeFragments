namespace AspnetcoreEx.GraphQL;

public interface IUserRepository : IAsyncDisposable
{
    User? GetUserById(int id);
    List<User> GetAllUser();
    IQueryable<User> GetAllUserQueryable();
    List<User> AddUser(User user);
    Task<List<User>> GetUserByIds(IEnumerable<int> keys);
}
public class UserRepository : IUserRepository
{
    public UserRepository(IDbContextFactory<GraphQLDbContext> dbContextFactory)
    {
        this.graphQLDbContext = dbContextFactory.CreateDbContext();
        this._dbSet = graphQLDbContext.Set<User>();
    }
    private readonly GraphQLDbContext graphQLDbContext;
    protected readonly DbSet<User> _dbSet;

    public List<User> AddUser(User user)
    {
        _dbSet.Add(user);
        graphQLDbContext.SaveChanges();
        return [.. graphQLDbContext.Users];
    }

    public List<User> GetAllUser()
    {
        return [.. _dbSet];
    }

    public User? GetUserById(int id)
    {
        return _dbSet.FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<User>> GetUserByIds(IEnumerable<int> keys)
    {
        return await _dbSet.Where(p => keys.Contains(p.Id)).ToListAsync();
    }

    public IQueryable<User> GetAllUserQueryable()
    {
        return _dbSet;
    }

    public ValueTask DisposeAsync()
    {
        return graphQLDbContext.DisposeAsync();
    }
}