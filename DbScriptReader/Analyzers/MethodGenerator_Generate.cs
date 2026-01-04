using Microsoft.CodeAnalysis;
using Scriban;
using System.Reflection;

namespace DbScriptReader.Analyzers
{
    public partial class MethodGenerator
    {
        private const string TemplatePath = "Files.Template.txt";

        private static string GetTemplateText()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{TemplatePath}");

            using StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        private static void Generate(SourceProductionContext context, MethodAttributeValues values)
        {
            string templateText = GetTemplateText();

            Template template = Template.Parse(templateText, TemplatePath);

            string code = template.Render(values);

            // This part is added in case of duplicate method/class name.
            string unique = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Path of generated source file.
            string hintName = $"{value.ClassName}/{value.MethodName}_{unique}.g.cs";

            context.AddSource(hintName, code);
        }
    }
}
