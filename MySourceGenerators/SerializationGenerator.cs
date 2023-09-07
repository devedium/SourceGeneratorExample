using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace MySourceGenerators
{
    [Generator]
    public class SerializationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            StringBuilder sourceBuilder = new StringBuilder();

            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var model = context.Compilation.GetSemanticModel(syntaxTree);

                // Look for classes with the AutoSerialize attribute
                var classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classDecl in classes)
                {
                    var classSymbol = model.GetDeclaredSymbol(classDecl);
                    if (classSymbol is INamedTypeSymbol)
                    {
                        var autoSerializeAttribute = classSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Name == "AutoSerializeAttribute");

                        if (autoSerializeAttribute != null)
                        {
                            sourceBuilder.Append(GenerateSerializationCode(classSymbol as INamedTypeSymbol));
                        }
                    }
                }
            }

            context.AddSource("SerializationGenerated", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private string GenerateSerializationCode(INamedTypeSymbol classSymbol)
        {
            // Create the namespace
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(classSymbol.ContainingNamespace.ToDisplayString()))
                .AddMembers(CreateClass(classSymbol));

            // Build the syntax tree
            var syntaxTree = SyntaxFactory.SyntaxTree(namespaceDeclaration);
            var workspace = new AdhocWorkspace();
            var formattedRoot = Formatter.Format(syntaxTree.GetRoot(), workspace);
            string formattedCode = formattedRoot.ToFullString();

            return formattedCode;
        }

        private ClassDeclarationSyntax CreateClass(INamedTypeSymbol classSymbol)
        {
            // Create the Serialize method
            var serializeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), "Serialize")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.SingletonList<StatementSyntax>(
                        SyntaxFactory.ReturnStatement(CreateSerializationExpression(classSymbol))
                    )
                ));

            // Create the class declaration
            var classDeclaration = SyntaxFactory.ClassDeclaration(classSymbol.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(serializeMethod);

            return classDeclaration;
        }        

        private ExpressionSyntax CreateSerializationExpression(INamedTypeSymbol classSymbol)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"$@""{{");
            var properties = classSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                if (i != properties.Count - 1)
                {
                    if (property.Type.SpecialType == SpecialType.System_String)
                    {
                        sb.Append($@"""""{property.Name}"""":""""{{{property.Name}}}"""",");
                    }
                    else
                    {
                        sb.Append($@"""""{property.Name}"""":{{{property.Name}}},");
                    }
                }
                else
                {
                    if (property.Type.SpecialType == SpecialType.System_String)
                    {
                        sb.Append($@"""""{property.Name}"""":""""{{{property.Name}}}""""");
                    }
                    else
                    {
                        sb.Append($@"""""{property.Name}"""":{{{property.Name}}}");
                    }
                }
            }
            sb.AppendLine(@"}}"";");

            return SyntaxFactory.ParseExpression(sb.ToString());
        }

        private string GenerateSerializationCode_v1(INamedTypeSymbol classSymbol)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"namespace {classSymbol.ContainingNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {classSymbol.Name}");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Serialize()");
            sb.AppendLine("        {");
            sb.Append(@"            return $@""{{");

            var properties = classSymbol.GetMembers().OfType<IPropertySymbol>().ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                if (i != properties.Count - 1)
                {
                    if (property.Type.SpecialType == SpecialType.System_String)
                    {
                        sb.Append($@"""""{property.Name}"""":""""{{{property.Name}}}"""",");
                    }
                    else
                    {
                        sb.Append($@"""""{property.Name}"""":{{{property.Name}}},");
                    }
                }
                else
                {
                    if (property.Type.SpecialType == SpecialType.System_String)
                    {
                        sb.Append($@"""""{property.Name}"""":""""{{{property.Name}}}""""");
                    }
                    else
                    {
                        sb.Append($@"""""{property.Name}"""":{{{property.Name}}}");
                    }
                }
            }
            sb.AppendLine(@"}}"";");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

    }
}
