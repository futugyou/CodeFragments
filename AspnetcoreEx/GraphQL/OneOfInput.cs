namespace AspnetcoreEx.GraphQL;

[OneOf]
public class PetInput
{
    public Dog? Dog { get; set; }

    public Cat? Cat { get; set; }
}
// [InterfaceType("IPet")]
public interface IPet
{
    string Name { get; }
}

public class Dog : IPet
{
    public string Name { get; set; }
}

public class Cat : IPet
{
    public string Name { get; set; }
}
