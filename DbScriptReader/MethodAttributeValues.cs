using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DbScriptReader
{
    // Value types are more cache friendly.
    // See https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md.

    internal readonly struct MethodAttributeValues
    {
        private static void GetHierarchy(ClassDeclarationSyntax classDeclaration, out string header, out string footer)
        {
            Stack<string> stack = new Stack<string>();

            int depth = 0;

            SyntaxNode? currentNode = classDeclaration;

            while (currentNode is ClassDeclarationSyntax cd)
            {
                stack.Push("\r\n{\r\n");
                stack.Push(cd.Identifier.ToString());
                stack.Push("class");
                stack.Push(cd.Modifiers.Join(" "));

                depth++;

                currentNode = currentNode.Parent;
            }

            if (currentNode is NamespaceDeclarationSyntax nd)
            {
                stack.Push("\r\n{\r\n");
                stack.Push(nd.Name.ToString());
                stack.Push("namespace");
                stack.Push(nd.Modifiers.Join(" "));

                depth++;
            }

            header = string.Join(" ", stack);
            footer = new string('}', depth);

            stack.Clear();
        }

        public readonly string? ClassFooter;
        public readonly string? ClassHeader;
        public readonly string? ClassName;
        public readonly string? MethodModifiers;
        public readonly string? MethodName;
        public readonly string? MethodReturnType;
        public readonly string? NewMethodModifiers;
        public readonly string? ParametersNames;
        public readonly string? ParametersSyntax;
        public readonly string? ScriptPath;

        public MethodAttributeValues
        (
            MethodDeclarationSyntax methodDeclaration,
            ClassDeclarationSyntax classDeclaration,
            AttributeSyntax attribute
        )
        {
            GetHierarchy(classDeclaration, out ClassHeader, out ClassFooter);

            ClassName = classDeclaration
                .Identifier
                .ToString();

            MethodModifiers = methodDeclaration
                .Modifiers
                .Select(modifier => modifier.Text)
                .Join(" ");

            MethodName = methodDeclaration
                .Identifier
                .ToString();

            MethodReturnType = methodDeclaration
                .ReturnType
                .ToString();

            NewMethodModifiers = methodDeclaration
                .Modifiers
                .Where(modifier => !modifier.IsKind(SyntaxKind.PartialKeyword))
                .Select(modifier => modifier.Text)
                .Join(" ");

            ParametersNames = methodDeclaration
                .ParameterList
                .Parameters
                .Select(parameter => parameter.Identifier)
                .Join(", ");

            ParametersSyntax = methodDeclaration
                .ParameterList
                .ToString()
                .Trim('(', ')');

            ScriptPath = attribute
                .ArgumentList?
                .Arguments
                .ToString();
        }
    }
}
