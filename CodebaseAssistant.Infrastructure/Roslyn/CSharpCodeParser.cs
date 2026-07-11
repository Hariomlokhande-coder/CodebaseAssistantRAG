using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodebaseAssistant.Infrastructure.Roslyn;

public class CSharpCodeParser : ICodeParser
{
    public async Task<List<ParsedCodeItem>> ParseAsync(string filePath)
    {
        var result = new List<ParsedCodeItem>();

        var code = await File.ReadAllTextAsync(filePath);

        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var root = await syntaxTree.GetRootAsync();

        var namespaceNode = root.DescendantNodes()
                                .OfType<NamespaceDeclarationSyntax>()
                                .FirstOrDefault();

        var namespaceName = namespaceNode?.Name.ToString() ?? string.Empty;

        var classes = root.DescendantNodes()
                          .OfType<ClassDeclarationSyntax>();

        foreach (var classNode in classes)
        {
            var methods = classNode.Members
                                   .OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                result.Add(new ParsedCodeItem
                {
                    FilePath = filePath,
                    Namespace = namespaceName,
                    ClassName = classNode.Identifier.Text,
                    MethodName = method.Identifier.Text,
                    Code = method.ToString()
                });
            }
        }

        return result;
    }
}