using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Demo
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DemoAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Demo";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString _title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _messageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString _description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Accessability";

        private static readonly DiagnosticDescriptor _rule = new DiagnosticDescriptor(DiagnosticId, _title,
                                                                                      _messageFormat, Category,
                                                                                      DiagnosticSeverity.Warning,
                                                                                      isEnabledByDefault: true,
                                                                                      description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        private static readonly IEnumerable<SyntaxKind> AllowedModifiers = new[] { SyntaxKind.AbstractKeyword, SyntaxKind.OverrideKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.NewKeyword, SyntaxKind.StaticKeyword, SyntaxKind.OperatorDeclaration, SyntaxKind.ExplicitKeyword, SyntaxKind.ImplicitKeyword, SyntaxKind.SealedKeyword, SyntaxKind.AsyncKeyword, SyntaxKind.ExternKeyword, SyntaxKind.ConstKeyword };

        public override void Initialize(AnalysisContext context)
        {
            if (context is null)
            {
                return;
            }

            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration,
                                             SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration,
                                             SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration,
                                             SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration,
                                             SyntaxKind.PropertyDeclaration, SyntaxKind.DelegateDeclaration);
        }

        /// <summary>
        /// Analyzes the <see cref="SyntaxNodeAnalysisContext.Node"/> if it has no accessability modifier
        /// </summary>
        /// <param name="context"></param>
        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.Parent.Kind() == SyntaxKind.InterfaceDeclaration)
            {
                return;
            }

            switch (context.Node)
            {
                case TypeDeclarationSyntax node:
                    HandleDefaultModifier(context, node.Modifiers, node.Identifier.GetLocation());
                    break;
                case FieldDeclarationSyntax node:
                    HandleDefaultModifier(context, node.Modifiers, node.Declaration.GetLocation());
                    break;
                case MethodDeclarationSyntax node:
                    HandleDefaultModifier(context, node.Modifiers, node.Identifier.GetLocation());
                    break;
                case PropertyDeclarationSyntax node:
                    HandleDefaultModifier(context, node.Modifiers, node.Identifier.GetLocation());
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Reports a diagnostic action on the node, if it has no accessability modifier
        /// </summary>
        /// <param name="context"><see cref="SyntaxNodeAnalysisContext"/> in which the diagnostic provider called.</param>
        /// <param name="modifiers">A <see cref="SyntaxTokenList"/> which contains all the modifiers of the node. e.g.: <code>sealed, virtual, explicit</code></param>
        /// <param name="location">A <see cref="Location"/> where the warning should appear</param>
        private static void HandleDefaultModifier(SyntaxNodeAnalysisContext context, SyntaxTokenList modifiers, Location location)
        {
            if (ConvertTokens(modifiers).Except(AllowedModifiers).Any())
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(_rule, location, DiagnosticSeverity.Info));
        }

        /// <summary>
        /// Converts the <see cref="SyntaxTokenList"/> to <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="syntaxes">A <see cref="SyntaxTokenList"/> which contains all the modifiers of a node.</param>
        /// <returns>The <see cref="IEnumerable{T}"/></returns>
        private static IEnumerable<SyntaxKind> ConvertTokens(SyntaxTokenList syntaxes)
        {
            return syntaxes.Select(s => s.Kind());
        }
    }
}
