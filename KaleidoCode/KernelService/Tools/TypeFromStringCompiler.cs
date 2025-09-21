using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KaleidoCode.KernelService.Tools;

public static class TypeFromStringCompiler
{
    private static readonly string[] DefaultUsings =
    [
        "System",
        "System.Text.Json.Serialization",
        "System.ComponentModel"
    ];

    public static Type? GenerateTypeFromClassBody(string classBody, string? className = null)
    {
        var usings = string.Join(Environment.NewLine, DefaultUsings.Select(u => $"using {u};"));

        var fullCode = $"""
        {usings}

        {classBody}
        """;

        var syntaxTree = CSharpSyntaxTree.ParseText(fullCode);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create(
            $"InMemoryTypes_{Guid.NewGuid()}",
           syntaxTrees: [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = string.Join("\n", result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));

            throw new InvalidOperationException($"Compilation failed: \n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        if (className == null)
        {
            var root = syntaxTree.GetRoot();
            className = root.DescendantNodes()
                            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
                            .FirstOrDefault()
                            ?.Identifier.Text;
        }

        return assembly.GetTypes().FirstOrDefault(t => t.Name == className);
    }
}
