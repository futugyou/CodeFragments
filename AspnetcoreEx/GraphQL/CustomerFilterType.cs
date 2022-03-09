using HotChocolate.Data.Filters;

namespace AspnetcoreEx.GraphQL;

public class UserFilterType : FilterInputType<User>
{
    protected override void Configure(IFilterInputTypeDescriptor<User> descriptor)
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


public class CustomFilterConvention : FilterConvention
{
    protected override void Configure(IFilterConventionDescriptor descriptor)
    {
        descriptor.AddDefaults();
        descriptor.AllowAnd(false).AllowOr(false);
    }
}
public class CustomFilterConventionExtension : FilterConventionExtension
{
    protected override void Configure(IFilterConventionDescriptor descriptor)
    {
        // this will instead of 'where'
        descriptor.ArgumentName("filter");
        descriptor.AllowAnd(false).AllowOr(false);
    }
}
