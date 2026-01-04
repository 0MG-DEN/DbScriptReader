using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DbScriptReader.Analyzers
{
    internal static class Shared
    {
        private const string FullName = "DbScriptFileAttribute";
        private const string ShortName = "DbScriptFile";

        public static IEnumerable<AttributeSyntax> GetAttributes(MethodDeclarationSyntax methodDeclaration)
        {
            foreach (AttributeListSyntax attributeList in methodDeclaration.AttributeLists)
            {
                foreach (AttributeSyntax attribute in attributeList.Attributes)
                {
                    string name = attribute.Name.ToString();

                    if (name == FullName || name == ShortName)
                    {
                        yield return attribute;
                    }
                }
            }
        }
    }
}
