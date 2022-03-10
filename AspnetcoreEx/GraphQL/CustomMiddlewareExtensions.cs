namespace AspnetcoreEx.GraphQL;

public static class CustomMiddlewareExtensions
{
    public static IObjectFieldDescriptor UseConsoleLogMiddleware(this IObjectFieldDescriptor descriptor)
    {
        return descriptor
              .Use(next => async context =>
              {
                  Console.WriteLine("GraphQLMiddleware Log before: " + context.Document);
                  await next(context);
                  Console.WriteLine("GraphQLMiddleware Log after: " + context.Result);
                  // Omitted code for brevity
              });
    }
}
