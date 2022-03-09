using HotChocolate.Data.Filters;

namespace AspnetcoreEx.GraphQL;

public class UserFilterType : FilterInputType<User>
{
    protected override void Configure(IFilterInputTypeDescriptor<User> descriptor)
    {
        descriptor.Name("CustomerUserFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(f => f.Name).Type<CustomerFilterInputType>();
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
