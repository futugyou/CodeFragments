using HotChocolate.Data.Sorting;

namespace AspnetcoreEx.GraphQL;

// public class UserSortType : SortInputType<User>
// {
//     protected override void Configure(ISortInputTypeDescriptor<User> descriptor)
//     {
//         descriptor.Name("CustomerUserSortType");
//         descriptor.BindFieldsExplicitly();
//         descriptor.Field(f => f.Name).Type<AscOnlySortEnumType>();
//     }
// }

// same reason as CustomerUserForFilter
public class CustomerUserForSort
{
    public string Name { get; set; }
}

public class CustomerUserSortType : SortInputType<CustomerUserForSort>
{
    protected override void Configure(ISortInputTypeDescriptor<CustomerUserForSort> descriptor)
    {
        descriptor.Name("CustomerUserSortType");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(f => f.Name).Type<AscOnlySortEnumType>();
    }
}

public class AscOnlySortEnumType : DefaultSortEnumType
{
    protected override void Configure(ISortEnumTypeDescriptor descriptor)
    {
        descriptor.Name("AscOnlySortEnumType");
        descriptor.Operation(DefaultSortOperations.Ascending);
    }
}

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