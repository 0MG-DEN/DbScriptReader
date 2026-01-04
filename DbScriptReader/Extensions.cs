using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;

namespace DbScriptReader
{
    internal static class Extensions
    {
        public static bool IsExtendable(this SyntaxTokenList modifiers, CancellationToken token)
        {
            bool isExtendable = false;

            foreach (SyntaxToken modifier in modifiers)
            {
                token.ThrowIfCancellationRequested();

                if (modifier.IsKind(SyntaxKind.PartialKeyword) && (isExtendable = true)) // Set flag and continue.
                    continue;

                if (modifier.IsKind(SyntaxKind.PrivateKeyword))
                    continue;

                if (modifier.IsKind(SyntaxKind.ProtectedKeyword))
                    continue;

                if (modifier.IsKind(SyntaxKind.InternalKeyword))
                    continue;

                if (modifier.IsKind(SyntaxKind.PublicKeyword))
                    continue;

                return false; // Contains some disallowed modifier.
            }

            return isExtendable;
        }

        public static bool IsExtendable(this ClassDeclarationSyntax classDeclaration, CancellationToken token)
        {
            SyntaxNode? currentNode = classDeclaration;

            while (currentNode is ClassDeclarationSyntax currentClass)
            {
                token.ThrowIfCancellationRequested();

                if (currentClass.Modifiers.IsExtendable(token))
                {
                    // Recursively visit class hierarchy
                    // and make sure each can be extended.
                    currentNode = currentNode.Parent;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsExtendable(this MethodDeclarationSyntax methodDeclaration, CancellationToken token)
        {
            return methodDeclaration.Modifiers.IsExtendable(token);
        }

        public static string Join(this IEnumerable values, string separator)
        {
            return string.Join(separator, values.OfType<object>());
        }
    }
}
