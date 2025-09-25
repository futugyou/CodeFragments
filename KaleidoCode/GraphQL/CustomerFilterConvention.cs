using HotChocolate.Data.Filters;

namespace KaleidoCode.GraphQL;


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
