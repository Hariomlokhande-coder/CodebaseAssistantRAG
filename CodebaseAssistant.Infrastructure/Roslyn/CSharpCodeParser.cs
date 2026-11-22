using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodebaseAssistant.Infrastructure.Roslyn;

public sealed class CSharpCodeParser : ICodeParser
{
    public async Task<List<ParsedCodeItem>> ParseAsync(string filePath)
    {
        var tree = CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(filePath));
        var root = await tree.GetRootAsync();
        var items = new List<ParsedCodeItem>();

        foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var namespaceName = type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>()
                .FirstOrDefault()?.Name.ToString() ?? string.Empty;

            Add(items, filePath, namespaceName, type.Identifier.Text, string.Empty, "Type", type);

            foreach (var member in type.Members)
            {
                switch (member)
                {
                    case MethodDeclarationSyntax method:
                        Add(items, filePath, namespaceName, type.Identifier.Text, method.Identifier.Text, "Method", method);
                        break;
                    case ConstructorDeclarationSyntax constructor:
                        Add(items, filePath, namespaceName, type.Identifier.Text, constructor.Identifier.Text, "Constructor", constructor);
                        break;
                    case PropertyDeclarationSyntax property:
                        Add(items, filePath, namespaceName, type.Identifier.Text, property.Identifier.Text, "Property", property);
                        break;
                }
            }
        }

        return items;
    }

    private static void Add(
        ICollection<ParsedCodeItem> items,
        string filePath,
        string namespaceName,
        string className,
        string methodName,
        string memberType,
        SyntaxNode node)
    {
        var lines = node.GetLocation().GetLineSpan();
        items.Add(new ParsedCodeItem
        {
            FilePath = filePath,
            Namespace = namespaceName,
            ClassName = className,
            MethodName = methodName,
            MemberType = memberType,
            Code = node.ToFullString(),
            StartLine = lines.StartLinePosition.Line + 1,
            EndLine = lines.EndLinePosition.Line + 1
        });
    }
}
