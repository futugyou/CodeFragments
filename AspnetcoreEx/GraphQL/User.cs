namespace AspnetcoreEx.GraphQL;

// [GraphQLName("user")]
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    // [GraphQLName("age")]
    public int Age { get; set; }
    // [GraphQLIgnore]
    // [GraphQLType(typeof(StringType))]
    public string Secret { get; set; }
    public List<Order> Orders { get; set; }
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