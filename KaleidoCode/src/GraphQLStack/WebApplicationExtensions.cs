
namespace GraphQLStack;

public static class GraphQLApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGraphQLCustom(this WebApplication app)
    {
        app.MapGraphQL().WithOptions(ops =>
        {
            ops.EnableSchemaRequests = true;
            // default is true
            ops.EnableGetRequests = true;
            // default is Query
            ops.AllowedGetOperations = AllowedGetOperations.Query;
            ops.EnableMultipartRequests = true;
            ops.Tool.Enable = true;
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