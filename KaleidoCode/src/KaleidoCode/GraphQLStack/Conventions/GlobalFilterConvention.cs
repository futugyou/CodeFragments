
namespace GraphQLStack.Conventions;

public class GlobalFilterConvention : FilterConvention
{
    protected override void Configure(IFilterConventionDescriptor descriptor)
    {
        descriptor.AddDefaults();
        descriptor.AllowAnd(false).AllowOr(false);
    }
}
public class GlobalFilterConventionExtension : FilterConventionExtension
{
    protected override void Configure(IFilterConventionDescriptor descriptor)
    {
        // this will instead of 'where'
        descriptor.ArgumentName("filter");
        descriptor.AllowAnd(false).AllowOr(false);
    }
}
