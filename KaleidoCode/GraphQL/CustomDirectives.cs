using HotChocolate.Resolvers;

namespace KaleidoCode.GraphQL;

// query{
//     user(id: 1){
//       id 
//       userName @my(name: "foo")
//     }
// }
// colsole print `name:"foo"`
// `context.Result` will be `tony`
// This function can be used for `aop` to process the `context.Result` according to specific `directive.Arguments`
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
            foreach (var item in directive.Arguments)
            {
                Console.WriteLine(item.Name + ":" + item.Value.ToString());
            }
            var task = next.Invoke(context);
            Console.WriteLine("CustomDirectiveType Got Result: " + context.Result);
            return task;
        });
    }
}
