
namespace GraphQLStack.Conventions;

public class GlobalSortConvention : SortConvention
{
    protected override void Configure(ISortConventionDescriptor descriptor)
    {
        descriptor.AddDefaults();
    }
}

public class GlobalSortConventionExtension : SortConventionExtension
{
    protected override void Configure(ISortConventionDescriptor descriptor)
    {
        // this will instead of 'order'
        descriptor.ArgumentName("sort");
    }
}