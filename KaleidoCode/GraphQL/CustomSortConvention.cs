using HotChocolate.Data.Sorting;

namespace KaleidoCode.GraphQL;

public class CustomSortConvention : SortConvention
{
    protected override void Configure(ISortConventionDescriptor descriptor)
    {
        descriptor.AddDefaults();
    }
}

public class CustomSortConventionExtension : SortConventionExtension
{
    protected override void Configure(ISortConventionDescriptor descriptor)
    {
        // this will instead of 'order'
        descriptor.ArgumentName("sort");
    }
}