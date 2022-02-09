namespace AspnetcoreEx.GraphQL;

public class Query
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
    public async Task<User?> GetUser(int id)
    {
        var user = DefaultUsers.FirstOrDefault(p => p.Id == id);
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
    public Task<List<User>> GetAllUser() => Task.FromResult(DefaultUsers);
}