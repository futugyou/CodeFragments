
namespace GraphQLStack;

public static class GraphQLApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGraphQLCustom(this WebApplication app)
    {
        app.MapGraphQL().WithOptions(new GraphQLServerOptions
        {
            // default is true, http://localhost:5000/graphql?sdl
            EnableSchemaRequests = true,
            // default is true
            EnableGetRequests = true,
            // default is Query
            AllowedGetOperations = AllowedGetOperations.Query,
            EnableMultipartRequests = true,
            Tool = {
                    Enable = true,
                },
        });
        // i think this is same as UseGraphQLVoyager
        // app.MapBananaCakePop("/graphql/ui").WithOptions(new GraphQLToolOptions
        // {
        //     UseBrowserUrlAsGraphQLEndpoint = true,
        //     GraphQLEndpoint = "/my/graphql/endpoint",
        // });
        app.UseGraphQLVoyager("/graphql-voyager", new VoyagerOptions { GraphQLEndPoint = "/graphql" });
        app.InitializeGraphQLDb();
        return app;
    }
}