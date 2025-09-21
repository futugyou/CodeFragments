using System.Reflection;
using System.Runtime.CompilerServices;
using HotChocolate.Resolvers;
using HotChocolate.Types.Descriptors;

namespace KaleidoCode.GraphQL;

public static class CustomMiddlewareExtensions
{
    public static IObjectFieldDescriptor UseConsoleLogMiddleware(this IObjectFieldDescriptor descriptor)
    {
        return descriptor
              .Use(next => async context =>
              {
                  //Console.WriteLine("GraphQLMiddleware Log before: " + context.Document);
                  await next(context);
                  Console.WriteLine("GraphQLMiddleware Log after: " + context.Result);
                  // Omitted code for brevity
              });
    }
}

public class CustomLogMiddleware
{
    private readonly FieldDelegate _next;

    public CustomLogMiddleware(FieldDelegate next) // Singleton Service Add Here
    {
        _next = next;
    }

    // this method must be called InvokeAsync or Invoke
    public async Task InvokeAsync(IMiddlewareContext context) // Scoped Service Add Here
    {
        // Code up here is executed before the following middleware
        // and the actual field resolver

        // This invokes the next middleware
        // or if we are at the last middleware the field resolver
        //Console.WriteLine("GraphQLMiddleware Log before: " + context.Document);
        await _next(context);

        // Code down here is executed after all later middleware
        // and the actual field resolver has finished executing
        Console.WriteLine("GraphQLMiddleware Log after: " + context.Result);
    }
}

public class UseConsoleLogAttribute : ObjectFieldDescriptorAttribute
{
    public UseConsoleLogAttribute([CallerLineNumber] int order = 0)
    {
        Order = order;
    }

    protected override void OnConfigure(IDescriptorContext context,
        IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        //descriptor.UseConsoleLogMiddleware();
        descriptor.Use<CustomLogMiddleware>();
    }
}

