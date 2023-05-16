using HotChocolate.Resolvers;

namespace AspnetcoreEx.GraphQL;

public class CustomDirectiveType : DirectiveType
{
    protected override void Configure(IDirectiveTypeDescriptor descriptor)
    {
        descriptor.Name("my");
        descriptor.Location(DirectiveLocation.Field);
        descriptor.Repeatable();
        descriptor
            .Argument("name")
            .Type<NonNullType<StringType>>();

        descriptor.Use((FieldDelegate next, Directive directive) => context =>
        {

            var task = next.Invoke(context);
            Console.WriteLine("CustomDirectiveType Got Result: " + context.Result);
            return task;
        });
    }
}
