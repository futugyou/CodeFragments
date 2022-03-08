using HotChocolate.Types.Pagination;
using HotChocolate.Subscriptions.Redis;
using StackExchange.Redis;

namespace AspnetcoreEx.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<UserRefetchableService>();
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
        .AddQueryType<Query>()
        .AddMutationType<Mutation>()
        .AddSubscriptionType<Subscription>()
        // .AddQueryType<QueryType>()
        // .AddMutationType<MutationType>()
        // .AddMutationType<SubscriptionType>()
        .ModifyRequestOptions(option =>
        {
            option.IncludeExceptionDetails = true;
        })
        .ModifyOptions(option =>
        {
            option.EnableOneOf = true;
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
        .AddInMemorySubscriptions()
        //.AddType(new UuidType('D'))
        .BindRuntimeType<string, StringType>()
        .AddType<Dog>()
        .AddType<Cat>()
        .AddType<UserRefetchable>()
        .AddTypeExtension<UserExtension>()
        .AddTypeExtension<QueryUserResolvers>()
        .AddDirectiveType<CustomDirectiveType>()
        .AddGlobalObjectIdentification()
        // .AddQueryFieldToMutationPayloads()
        //.AddRedisSubscriptions((sp) => ConnectionMultiplexer.Connect("host:port"))
        ;
        return services;
    }
}