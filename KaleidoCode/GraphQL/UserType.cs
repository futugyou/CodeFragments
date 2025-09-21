namespace KaleidoCode.GraphQL;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Name("user");
        descriptor.Field(f => f.Age).Name("age");
        descriptor.Description("Represents User Entity Type.");
        // ignore 'Secret' when build schema.
        descriptor.Field(d => d.Secret).Ignore();
        //descriptor.Ignore(f => f.Secret);
    }
}