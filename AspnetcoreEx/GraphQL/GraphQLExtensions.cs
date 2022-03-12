using HotChocolate.Types.Pagination;
using HotChocolate.Subscriptions.Redis;
using StackExchange.Redis;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;

namespace AspnetcoreEx.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddTransient<UserRefetchableService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddPooledDbContextFactory<GraphQLDbContext>(b =>
        {
            b.UseInMemoryDatabase("GraphQLDb");
        });
        services.AddHttpContextAccessor();
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
        .AddSpatialTypes()
        .AddSpatialProjections()
        .AddSpatialFiltering()
        .AddHttpRequestInterceptor<HttpRequestInterceptor>()
        .AddSocketSessionInterceptor<SocketSessionInterceptor>()
        //.AllowIntrospection(env.IsDevelopment())
        .AddType<UploadType>()
        // .AddConvention<IFilterConvention, CustomFilterConvention>()
        // .AddConvention<IFilterConvention, CustomFilterConventionExtension>()
        // .AddConvention<ISortConvention, CustomSortConvention>()
        // .AddConvention<ISortConvention, CustomSortConventionExtension>()
        // .AddQueryFieldToMutationPayloads()
        // .AddRedisSubscriptions((sp) => ConnectionMultiplexer.Connect("host:port"))
        ;
        services.Configure<FormOptions>(options =>
        {
            // Set the limit to 256 MB
            options.MultipartBodyLengthLimit = 268435456;
        });
        return services;
    }
}