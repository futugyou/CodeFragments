
using HotChocolate.Language;
using Microsoft.AspNetCore.Http.Features;

namespace KaleidoCode.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        // Global Services
        services.Configure<GraphQLOptions>(configuration.GetSection("GraphQLOptions"));
        var config = configuration.GetSection("GraphQLOptions").Get<GraphQLOptions>() ?? new();

        services.AddTransient<UserRefetchableService>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddPooledDbContextFactory<GraphQLDbContext>(b =>
        {
            b.UseInMemoryDatabase("GraphQLDb");
        });
        services.AddHttpContextAccessor();
        services.AddMemoryCache();


        // choose one of the following providers
        // services.AddMD5DocumentHashProvider(HashFormat.Hex);
        services.AddSha256DocumentHashProvider(HashFormat.Hex);
        // services.AddSha1DocumentHashProvider();

        var hotChocolateBuilder = services.AddGraphQLServer().AddInstrumentation();

        // code generates
        // It's strange that the two types `Cat` and `UserRefetchable` are not recognized by `HotChocolate.Types.Analyzers`.
        hotChocolateBuilder.AddHotGraphQL();

        // base
        hotChocolateBuilder
            .AddAuthorization()
            .AddFiltering()
            .AddProjections() // AddProjections can get include data like ef.
            .AddSorting()
            .ModifyPagingOptions(op =>
            {
                op.MaxPageSize = 50;
                op.IncludeTotalCount = true;
            })
            .ModifyRequestOptions(option =>
            {
                option.IncludeExceptionDetails = true;
            })
            .ModifyOptions(option =>
            {
                option.EnableOneOf = true;
            });

        // .AddSpatialTypes, Currently not used in the project.
        hotChocolateBuilder
            .AddSpatialTypes()
            .AddSpatialProjections()
            .AddSpatialFiltering();

        // .AddTypes
        hotChocolateBuilder
            .AddType<UploadType>()
            .AddType(new UuidType('D'))
            .AddType<Cat>() // type of `oneof`
            .AddType<UserRefetchable>() // type of `node`
            .AddDirectiveType<CustomDirectiveType>();

        // type converter
        hotChocolateBuilder
            // this will add `node`
            .AddGlobalObjectIdentification();

        // object types
        if (config.DevPattern == "Code")
        {
            hotChocolateBuilder
                .AddQueryType<QueryType>()
                .AddMutationType<MutationType>()
                .AddSubscriptionType<SubscriptionType>();

        }
        else
        {
            hotChocolateBuilder
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddSubscriptionType<Subscription>();
        }

        hotChocolateBuilder.AddPostgresSubscriptions((sp, options) =>
        {
            options.ConnectionFactory = ct => ValueTask.FromResult(new Npgsql.NpgsqlConnection(configuration.GetConnectionString("Postgres")!));
        });

        hotChocolateBuilder.AddMutationConventions(new MutationConventionOptions
        {
            ApplyToAllMutations = true,
            InputArgumentName = "input",
            InputTypeNamePattern = "{MutationName}Input",
            PayloadTypeNamePattern = "{MutationName}Payload",
            PayloadErrorTypeNamePattern = "{MutationName}Error",
            PayloadErrorsFieldName = "errors"
        })
        .BindRuntimeType<string, StringType>()
        .AddHttpRequestInterceptor<HttpRequestInterceptor>()
        .AddSocketSessionInterceptor<SocketSessionInterceptor>()
        //.AllowIntrospection(env.IsDevelopment())
        // .AddRemoteSchema(WellKnownSchemaNames.Myself)
        // .AddRemoteSchema(WellKnownSchemaNames.Myself2)
        // .AddRemoteSchemasFromRedis("Demo", sp => sp.GetRequiredService<ConnectionMultiplexer>())
        .InitializeOnStartup()
        .UseAutomaticPersistedOperationPipeline()
        .AddInMemoryOperationDocumentStorage()
        // .AddRedisQueryStorage(services => ConnectionMultiplexer.Connect("redisstring").GetDatabase())
        // .UsePersistedQueryPipeline()
        // .AddReadOnlyFileSystemQueryStorage("./persisted_queries")
        // .AddReadOnlyRedisQueryStorage(services => ConnectionMultiplexer.Connect("redisstring").GetDatabase())
        // .PublishSchemaDefinition(c => c
        //     // The name of the schema. This name should be unique
        //     .SetName("users")
        //     // Ignore the root types of accounts
        //     .IgnoreRootTypes()
        //     // Declares where the type extension is used
        //     .AddTypeExtensionsFromFile("./Stitching.graphql")
        //     .PublishToRedis(
        //         // The configuration name under which the schema should be published
        //         "Demo",
        //         // The connection multiplexer that should be used for publishing
        //         sp => sp.GetRequiredService<ConnectionMultiplexer>()
        //         )
        // )
        // .AddConvention<IFilterConvention, CustomFilterConvention>()
        // .AddConvention<IFilterConvention, CustomFilterConventionExtension>()
        // .AddConvention<ISortConvention, CustomSortConvention>()
        // .AddConvention<ISortConvention, CustomSortConventionExtension>()
        // .AddQueryFieldToMutationPayloads()
        ;
        services.Configure<FormOptions>(options =>
        {
            // Set the limit to 256 MB
            options.MultipartBodyLengthLimit = 268435456;
        });
        return services;
    }
}