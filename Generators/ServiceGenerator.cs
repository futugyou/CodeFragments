using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generators
{
    //DOTO:  it does not work!
    [Generator]
    public class ServiceGenerator : ISourceGenerator
    {
        private const string code = @"
namespace HelloGenerated
{
    public class HelloGenerator
    {
        public static void Test() => System.Console.WriteLine(""Hello Generator"");
    }
}
";
        public void Initialize(GeneratorInitializationContext context)
        {
            //context.RegisterForPostInitialization(post =>
            //{
            //    post.AddSource("ServiceGeneratorRegisterForPostInitialization", code);
            //});
            //context.RegisterForSyntaxNotifications(() => new PrivateSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var code = @"
namespace HelloGenerated
{
    public class HelloGenerator
    {
        public static void Test() => System.Console.WriteLine(""Hello ServiceGenerator"");
    }
}";
            context.AddSource(nameof(ServiceGenerator), code);
        }
    }
}

internal class PrivateSyntaxReceiver : ISyntaxReceiver
{
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {

    }
}
