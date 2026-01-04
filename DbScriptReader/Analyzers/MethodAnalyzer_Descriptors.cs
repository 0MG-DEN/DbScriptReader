using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DbScriptReader.Analyzers
{
    public partial class MethodAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableArray<DiagnosticDescriptor> Descriptors = ImmutableArray.Create
        (
            new DiagnosticDescriptor
            (
                "DSR1001",
                "Method is not extendable",
                "Method {0} is not extendable, it has modifiers other than public, protected, private, internal or is not partial. Skipped.",
                "Warning",
                DiagnosticSeverity.Warning,
                true
            ),
            new DiagnosticDescriptor
            (
                "DSR1002",
                "Class is not extendable",
                "Class {0} is not extendable, it (or its containg classes) has modifiers other than public, protected, private, internal or is not partial. Skipped.",
                "Warning",
                DiagnosticSeverity.Warning,
                true
            ),
            new DiagnosticDescriptor
            (
                "DSR2001",
                "Invalid return type",
                "Method {1} has invalid return type: {0}. Should be string.",
                "Error",
                DiagnosticSeverity.Error,
                true
            ),
            new DiagnosticDescriptor
            (
                "DSR2002",
                "Missing arguments list",
                "Attribute {0} is missing an arguments list.",
                "Error",
                DiagnosticSeverity.Error,
                true
            )
        );

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors;
    }
}
