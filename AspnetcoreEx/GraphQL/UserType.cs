namespace AspnetcoreEx.GraphQL;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Description("Represents User Entity Type.");
        // ignore 'Secret' when build schema.
        descriptor.Field(d => d.Secret).Ignore();
    }
}