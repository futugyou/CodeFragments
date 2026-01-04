
namespace GraphQLStack.Users;

public class UserBatchDataLoader : BatchDataLoader<int, User>
{
    private readonly IUserRepository _repository;

    public UserBatchDataLoader(
        IUserRepository repository,
        IBatchScheduler batchScheduler,
        DataLoaderOptions options)
        : base(batchScheduler, options)
    {
        _repository = repository;
    }

    protected override async Task<IReadOnlyDictionary<int, User>> LoadBatchAsync(IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        // instead of fetching one person, we fetch multiple persons
        var users = await _repository.GetUserByIds(keys);
        return users.ToDictionary(x => x.Id);
    }
}