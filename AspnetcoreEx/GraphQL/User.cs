using NetTopologySuite.Geometries;

namespace AspnetcoreEx.GraphQL;

//[Authorize]
// [Authorize(Roles = new[] { "Guest", "Administrator" })]
// [Authorize(Policy = "AtLeast21")]
public class AuthUser
{
    public string UserID { get; set; }
    public string Name { get; set; }
}

// [GraphQLName("user")]
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    // [GraphQLName("age")]
    public int Age { get; set; }
    // [GraphQLIgnore]
    // [GraphQLType(typeof(StringType))]
    public string? Secret { get; set; }
    public Point? Location { get; set; }
    public List<Order> Orders { get; set; }

    // query {
    //   user(id: 1)
    //     {
    //         friends {
    //             id
    //             age
    //           userName
    //         }
    //     }
    // }
    public List<User> GetFriends([Service] IUserRepository repository)
    {
        var currentUserId = this.Id;
        var all = repository.GetAllUser();
        return [.. all.Where(p => p.Id != currentUserId)];
    }
}

public class Order
{
    public int Id { get; set; }
    public DateTime OrderTime { get; set; }
    public List<Product> Products { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public double Price { get; set; }
    public string ProductName { get; set; }
}