using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Demo
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DemoCodeFixProvider)), Shared]
    public class DemoCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Remove private accessability modifier";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DemoAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            SyntaxToken syntax = root.FindToken(diagnosticSpan.Start);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => Fix(context.Document, syntax, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        async Task<Document> Fix(Document document, SyntaxToken syntax, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxToken nextToken = syntax.GetNextToken();
            return document.WithSyntaxRoot(root
                .ReplaceTokens(new[] { syntax, nextToken }, (t, _) =>
                {
                    if (t == syntax)
                        return SyntaxFactory.Token(SyntaxKind.None);
                    if (t == nextToken)
                        return nextToken.WithLeadingTrivia(syntax.LeadingTrivia.AddRange(nextToken.LeadingTrivia));
                    return default;
                }));
        }
    }
}
