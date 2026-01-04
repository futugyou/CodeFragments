
namespace GraphQLStack.Users;

// public class UserFilterType : FilterInputType<User>
// {
//     protected override void Configure(IFilterInputTypeDescriptor<User> descriptor)
//     {
//         descriptor.Name("CustomerUserFilterInput");
//         descriptor.BindFieldsExplicitly();
//         descriptor.Field(f => f.Name).Type<CustomerFilterInputType>();
//         descriptor.AllowAnd(false).AllowOr(false);
//     }
// }

// Why use UserFilter instead of UserFilterType? 
// Because UserFilterType inherits FilterInputType<User>, this `User` will replace the default [UseFiltering] with [UseFiltering(typeof(UserFilterType))]
public class UserFilter
{
    public string Name { get; set; }
}
public class CustomerUserFilterType : FilterInputType<UserFilter>
{
    protected override void Configure(IFilterInputTypeDescriptor<UserFilter> descriptor)
    {
        descriptor.Name("CustomerUserFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(f => f.Name).Type<CustomerFilterInputType>();
        descriptor.AllowAnd(false).AllowOr(false);
    }
}

public class CustomerFilterInputType : StringOperationFilterInputType
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        descriptor.Name("CustomerFilterInputType");
        descriptor.Operation(DefaultFilterOperations.Equals).Type<StringType>();
        //descriptor.Operation(DefaultFilterOperations.NotEquals).Type<StringType>();
    }
}
