
using KaleidoCode.GraphQL.Users;

namespace KaleidoCode.GraphQL;
public class GraphQLDbContext : DbContext
{
    public GraphQLDbContext(DbContextOptions<GraphQLDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}

public static class GraphQLDbInitializer
{
    public static void InitializeGraphQLDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = app.Services;
        var factory = services.GetRequiredService<IDbContextFactory<GraphQLDbContext>>();
        var context = factory.CreateDbContext();
        Initialize(context);
    }

    public static void Initialize(GraphQLDbContext context)
    {
        context.Database.EnsureCreated();

        // Look for any students.
        if (context.Users.Any())
        {
            return;   // DB has been seeded
        }

        var DefaultProducts = new List<Product>{
            new () { Id = 1, Price = 1.1, ProductName = "apple" },
            new () { Id = 2, Price = 1.2, ProductName = "milk" },
            new () { Id = 3, Price = 1.3, ProductName = "beef" },
            new () { Id = 4, Price = 1.4, ProductName = "table" },
            new () { Id = 5, Price = 1.5, ProductName = "phone" },
        };
        var DefaultOrders = new List<Order>{
            new () { Id = 1, OrderTime = DateTime.Now, Products = [.. DefaultProducts.Take(3)]},
            new () { Id = 2, OrderTime = DateTime.Now.AddHours(-1), Products = [.. DefaultProducts.Skip(1).Take(3)]},
            new () { Id = 3, OrderTime = DateTime.Now.AddHours(-2), Products = [.. DefaultProducts.Skip(2).Take(3)]},
            new () { Id = 4, OrderTime = DateTime.Now.AddHours(-3), Products = [.. DefaultProducts.Skip(3).Take(3)]},
        };
        var DefaultUsers = new List<User> {
            new () { Id = 1, Name = "tony", Age = 28, Secret = "not like tom", Location = new NetTopologySuite.Geometries.Point(29.098,39.098), Orders = [.. DefaultOrders.Take(2)]},
            new () { Id = 2, Name = "tom", Age = 23, Secret = "not like tony", Location = new NetTopologySuite.Geometries.Point(34.864,57.356),  Orders = [.. DefaultOrders.Skip(2).Take(2)]},
        };
        context.Users.AddRange(DefaultUsers);
        context.SaveChanges();
    }
}