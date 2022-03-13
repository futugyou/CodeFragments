using HotChocolate.Types.Pagination;
using HotChocolate.Subscriptions.Redis;
using StackExchange.Redis;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using HotChocolate.Language;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspnetcoreEx.GraphQL;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQL(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("MySuperSecretKey"));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidIssuer = "https://auth.chillicream.com",
                        ValidAudience = "https://graphql.chillicream.com",
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey
                    };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AtLeast21", policy =>
                policy.Requirements.Add(new MinimumAgeRequirement(21)));

            options.AddPolicy("HasCountry", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == ClaimTypes.Country)));
        });
        services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
        // This is the connection multiplexer that redis will use
        // services.AddSingleton(ConnectionMultiplexer.Connect("redisstring"));
        services.AddTransient<UserRefetchableService>();
        services.AddScoped<IUserRepository, UserRepository>();
        // services.AddHttpClient(WellKnownSchemaNames.Myself, c => c.BaseAddress = new Uri("http://localhost:5001/graphql"));
        // services.AddHttpClient(WellKnownSchemaNames.Myself2, c => c.BaseAddress = new Uri("http://localhost:5001/graphql"));
        services.AddPooledDbContextFactory<GraphQLDbContext>(b =>
        {
            b.UseInMemoryDatabase("GraphQLDb");
        });
        services.AddHttpContextAccessor();
        // Global Services
        services.AddMemoryCache();
        // choose one of the following providers
        // services.AddMD5DocumentHashProvider(HashFormat.Hex);
        services.AddSha256DocumentHashProvider(HashFormat.Hex);
        // services.AddSha1DocumentHashProvider();

        services
        .AddGraphQLServer()
        .AddAuthorization()
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
        // .AddRemoteSchema(WellKnownSchemaNames.Myself)
        // .AddRemoteSchema(WellKnownSchemaNames.Myself2)
        // .AddRemoteSchemasFromRedis("Demo", sp => sp.GetRequiredService<ConnectionMultiplexer>())
        .InitializeOnStartup()
        .UseAutomaticPersistedQueryPipeline()
        .AddInMemoryQueryStorage()
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