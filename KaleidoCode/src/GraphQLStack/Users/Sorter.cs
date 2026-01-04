
namespace GraphQLStack.Users;

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
public class UserSorter
{
    public string Name { get; set; }
}

public class CustomerUserSortType : SortInputType<UserSorter>
{
    protected override void Configure(ISortInputTypeDescriptor<UserSorter> descriptor)
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
