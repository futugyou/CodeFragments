using HotChocolate.Types.Pagination;

namespace AspnetcoreEx.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services, IConfiguration configuration)
    {
        services
        .AddGraphQLServer()
        .AddFiltering()
        .AddProjections() // AddProjections can get include data like ef.
        .AddSorting()
        .SetPagingOptions(new PagingOptions
        {
            MaxPageSize = 50,
            IncludeTotalCount = true
        })
        .AddQueryType<QueryType>()
        .AddMutationType<MutationType>()
        .ModifyRequestOptions(option =>
        {
            option.IncludeExceptionDetails = true;
        })
        // .AddMutationConventions(new MutationConventionOptions
        // {
        //     ApplyToAllMutations = true,
        //     InputArgumentName = "input",
        //     InputTypeNamePattern = "{MutationName}Input",
        //     PayloadTypeNamePattern = "{MutationName}Payload",
        //     PayloadErrorTypeNamePattern = "{MutationName}Error",
        //     PayloadErrorsFieldName = "errors"
        // })
        ;
        return services;
    }
}