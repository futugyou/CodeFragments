namespace AspnetcoreEx.GraphQL;

public interface IUserRepository
{
    User? GetUserById(int id);
    List<User> GetAllUser();
    List<User> AddUser(User user);
    Task<List<User>> GetUserByIds(IEnumerable<int> keys);
}
public class UserRepository : IUserRepository
{
    private static List<Product> DefaultProducts = new List<Product>{
        new Product{ Id = 1, Price = 1.1, ProductName = "apple" },
        new Product{ Id = 2, Price = 1.2, ProductName = "milk" },
        new Product{ Id = 3, Price = 1.3, ProductName = "beef" },
        new Product{ Id = 4, Price = 1.4, ProductName = "table" },
        new Product{ Id = 5, Price = 1.5, ProductName = "phone" },
    };
    private static List<Order> DefaultOrders = new List<Order>{
        new Order { Id = 1, OrderTime = DateTime.Now, Products = DefaultProducts.Take(3).ToList()},
        new Order { Id = 2, OrderTime = DateTime.Now.AddHours(-1), Products = DefaultProducts.Skip(1).Take(3).ToList()},
        new Order { Id = 3, OrderTime = DateTime.Now.AddHours(-2), Products = DefaultProducts.Skip(2).Take(3).ToList()},
        new Order { Id = 4, OrderTime = DateTime.Now.AddHours(-3), Products = DefaultProducts.Skip(3).Take(3).ToList()},
    };
    private static List<User> DefaultUsers = new List<User> {
        new User { Id = 1, Name = "tony", Age = 28, Secret = "not like tom", Orders = DefaultOrders.Take(2).ToList()},
        new User { Id = 2, Name = "tom", Age = 23, Secret = "not like tony", Orders = DefaultOrders.Skip(2).Take(2).ToList()},
    };

    public List<User> AddUser(User user)
    {
        DefaultUsers.Add(user);
        return DefaultUsers;
    }

    public List<User> GetAllUser()
    {
        return DefaultUsers;
    }

    public User? GetUserById(int id)
    {
        return DefaultUsers.FirstOrDefault(p => p.Id == id);
    }

    public Task<List<User>> GetUserByIds(IEnumerable<int> keys)
    {
        return Task.FromResult(DefaultUsers.Where(p => keys.Contains(p.Id)).ToList());
    }
}