
using HotChocolate.Language;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using Microsoft.AspNetCore.Http.Features;
using KaleidoCode.GraphQL.Conventions;
using KaleidoCode.GraphQL.Directives;
using KaleidoCode.GraphQL.Users;
using KaleidoCode.GraphQL.Pets;
using KaleidoCode.GraphQL.Interceptors;
using StackExchange.Redis;

namespace KaleidoCode.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        // Global Services
        services.Configure<GraphQLOptions>(configuration.GetSection("GraphQLOptions"));
        var config = configuration.GetSection("GraphQLOptions").Get<GraphQLOptions>() ?? new();

        services.AddTransient<NodeRefetchableService>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddPooledDbContextFactory<GraphQLDbContext>(b =>
        {
            b.UseInMemoryDatabase("GraphQLDb");
        });
        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        var hashFormat = HashFormat.Base64;
        if (config.HashFormat == "Hex")
        {
            hashFormat = HashFormat.Hex;
        }

        switch (config.HashProvider)
        {
            case "Sha256":
                services.AddSha256DocumentHashProvider(hashFormat);
                break;

            case "Sha1":
                services.AddSha1DocumentHashProvider(hashFormat);
                break;

            default:
                services.AddMD5DocumentHashProvider(hashFormat);
                break;
        }

        var hotChocolateBuilder = services
            .AddGraphQLServer()
            .AddInstrumentation()
            .BindRuntimeType<string, StringType>();

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
            .AddType<NodeRefetchable>() // type of `node`
            .AddDirectiveType<GlobalDirectiveType>();

        // type converter
        hotChocolateBuilder
            // this will add `node`
            .AddGlobalObjectIdentification();

        hotChocolateBuilder
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>()
            .AddQueryFieldToMutationPayloads();

        switch (config.SubscriptionOperations)
        {
            case "Postgres" when !string.IsNullOrEmpty(configuration.GetConnectionString("Postgres")):
                var postgres = configuration.GetConnectionString("Postgres");
                hotChocolateBuilder.AddPostgresSubscriptions((sp, options) =>
                {
                    options.ConnectionFactory = ct => ValueTask.FromResult(new Npgsql.NpgsqlConnection(postgres));
                });
                break;

            case "Redis" when !string.IsNullOrEmpty(configuration.GetConnectionString("Redis")):
                var redis = configuration.GetConnectionString("Redis")!;
                hotChocolateBuilder.AddRedisSubscriptions((sp) => ConnectionMultiplexer.Connect(redis));
                break;

            default:
                hotChocolateBuilder.AddInMemorySubscriptions();
                break;
        }

        hotChocolateBuilder.AddMutationConventions(new MutationConventionOptions
        {
            ApplyToAllMutations = true,
            InputArgumentName = "input",
            InputTypeNamePattern = "{MutationName}Input",
            PayloadTypeNamePattern = "{MutationName}Payload",
            PayloadErrorTypeNamePattern = "{MutationName}Error",
            PayloadErrorsFieldName = "errors"
        });

        if (config.UseNetInterceptor)
        {
            hotChocolateBuilder.AddHttpRequestInterceptor<HttpRequestInterceptor>();
            hotChocolateBuilder.AddSocketSessionInterceptor<SocketSessionInterceptor>();
        }

        hotChocolateBuilder.UseAutomaticPersistedOperationPipeline();

        switch (config.PersistedOperations)
        {
            case "FileSystem":
                hotChocolateBuilder.AddFileSystemOperationDocumentStorage("./persisted_operations");
                break;

            case "Redis" when !string.IsNullOrEmpty(configuration.GetConnectionString("Redis")):
                var redis = configuration.GetConnectionString("Redis")!;
                hotChocolateBuilder.AddRedisOperationDocumentStorage(
                    services => ConnectionMultiplexer.Connect(redis).GetDatabase());
                break;

            default:
                hotChocolateBuilder.AddInMemoryOperationDocumentStorage();
                break;
        }

        if (config.UseGlobalFilterConvention)
        {
            hotChocolateBuilder
                  // .AddConvention<IFilterConvention, GlobalFilterConvention>() or
                  .AddConvention<IFilterConvention, GlobalFilterConventionExtension>();
        }

        if (config.UseGlobalSortConvention)
        {
            hotChocolateBuilder
                // .AddConvention<ISortConvention, GlobalSortConvention>() or
                .AddConvention<ISortConvention, GlobalSortConventionExtension>();
        }

        services.Configure<FormOptions>(options =>
        {
            // Set the limit to 256 MB
            options.MultipartBodyLengthLimit = 268435456;
        });

        return services;
    }
}