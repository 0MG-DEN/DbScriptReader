using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DbScriptReader.Analyzers
{
    /// <summary>
    /// Generator that will extend methods attributed with <see cref="DbScriptFileAttribute"/> with
    /// additional overloads capable of execuiting and querying database using external script files.
    /// </summary>
    [Generator]
    public partial class MethodGenerator : IIncrementalGenerator
    {
        private static bool IsAttributedMethod(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // This predicate will be called every time there's a change in editor
            // (i.e. key is pressed) hence it should be very fast and not allocate.
            return node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0;
        }

        private static MethodAttributeValues? VisitNode(GeneratorSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not MethodDeclarationSyntax methodDeclaration)
                return default;

            if (methodDeclaration.Parent is not ClassDeclarationSyntax classDeclaration)
                return default;

            if (!methodDeclaration.IsExtendable(token))
                return default;

            if (!classDeclaration.IsExtendable(token))
                return default;

            AttributeSyntax attribute = Shared.GetAttributes(methodDeclaration).SingleOrDefault();

            if (attribute is null)
                return default;

            return new MethodAttributeValues(methodDeclaration, classDeclaration, attribute);
        }

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            static bool Predicate(MethodAttributeValues? values)
                => values.HasValue;

            static void Action(SourceProductionContext context, MethodAttributeValues? values)
                => Generate(context, values.GetValueOrDefault());

            IncrementalValuesProvider<MethodAttributeValues?> provider = context
                .SyntaxProvider
                .CreateSyntaxProvider(IsAttributedMethod, VisitNode)
                .Where(Predicate);

            context.RegisterSourceOutput(provider, Action);
        }
    }
}
