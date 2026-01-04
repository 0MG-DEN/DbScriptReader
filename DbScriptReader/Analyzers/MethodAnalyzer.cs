using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DbScriptReader.Analyzers
{
    // Diagnostics should not be reported via generator itself but rather through a dedicated analyzer.
    // See https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#issue-diagnostics.

    /// <summary>
    /// Helper analyzer that will report any invalid syntax that prevents usage of <see cref="DbScriptFileAttribute"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class MethodAnalyzer : DiagnosticAnalyzer
    {
        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not MethodDeclarationSyntax methodDeclaration)
                return;

            if (methodDeclaration.Parent is not ClassDeclarationSyntax classDeclaration)
                return;

            if (methodDeclaration.AttributeLists.Count < 1)
                return;

            AttributeSyntax[] attributes = Shared.GetAttributes(methodDeclaration).ToArray();

            if (attributes.Length != 1)
                return;

            AttributeSyntax attribute = attributes[0];

            // Now we can start gathering diagnostics
            // since we've found our custom attribute.

            if (!methodDeclaration.IsExtendable(default))
            {
                Diagnostic diagnostic = Diagnostic.Create
                (
                    Descriptors[0],
                    methodDeclaration.GetLocation(),
                    methodDeclaration.Identifier
                );
                context.ReportDiagnostic(diagnostic);
            }

            if (!classDeclaration.IsExtendable(default))
            {
                Diagnostic diagnostic = Diagnostic.Create
                (
                    Descriptors[1],
                    classDeclaration.GetLocation(),
                    classDeclaration.Identifier
                );
                context.ReportDiagnostic(diagnostic);
            }

            TypeSyntax returnType = methodDeclaration.ReturnType;
            SymbolInfo returnInfo = context.SemanticModel.GetSymbolInfo(returnType);

            if (returnInfo.Symbol is not INamedTypeSymbol namedTypeSymbol || namedTypeSymbol.Name != "String")
            {
                Diagnostic diagnostic = Diagnostic.Create
                (
                    Descriptors[2],
                    returnType.GetLocation(),
                    returnType.ToString(),
                    methodDeclaration.Identifier
                );
                context.ReportDiagnostic(diagnostic);
            }

            if (attribute.ArgumentList is not AttributeArgumentListSyntax attributeArgumentList)
            {
                Diagnostic diagnostic = Diagnostic.Create
                (
                    Descriptors[3],
                    attribute.GetLocation(),
                    attribute.Name
                );
                context.ReportDiagnostic(diagnostic);
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction<SyntaxKind>(Analyze, SyntaxKind.MethodDeclaration);
        }
    }
}
