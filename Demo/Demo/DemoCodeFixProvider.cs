using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DemoCodeFixProvider)), Shared]
    public class DemoCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add private accessability modifier";

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
            SyntaxNode syntax = root.FindToken(diagnosticSpan.Start).Parent;

            if (syntax is PredefinedTypeSyntax)
            {
                syntax = syntax.Parent.Parent;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => Fix(context.Document, syntax, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> Fix(Document document, SyntaxNode syntax, CancellationToken c)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);
            SyntaxNode newRoot;

            switch (syntax)
            {
                case TypeDeclarationSyntax node:
                    {
                        SyntaxToken token = Resolve(node.GetFirstToken());
                        SyntaxTokenList newList = node.Modifiers.Insert(0, token);

                        TypeDeclarationSyntax newNode = node.WithModifiers(newList);
                        newRoot = root.ReplaceNode(node, newNode);
                    }
                    break;
                case LocalDeclarationStatementSyntax node:
                    {
                        SyntaxToken token = Resolve(node.GetFirstToken());
                        SyntaxTokenList newList = node.Modifiers.Insert(0, token);

                        LocalDeclarationStatementSyntax newNode = node.WithModifiers(newList);
                        newRoot = root.ReplaceNode(node, newNode);
                    }
                    break;
                case MethodDeclarationSyntax node:
                    {
                        SyntaxToken token = Resolve(node.GetFirstToken());
                        SyntaxTokenList newList = node.Modifiers.Insert(0, token);

                        MethodDeclarationSyntax newNode = node.WithModifiers(newList);
                        newRoot = root.ReplaceNode(node, newNode);
                    }
                    break;
                case PropertyDeclarationSyntax node:
                    {
                        SyntaxToken token = Resolve(node.GetFirstToken());
                        SyntaxTokenList newList = node.Modifiers.Insert(0, token);

                        PropertyDeclarationSyntax newNode = node.WithModifiers(newList);
                        newRoot = root.ReplaceNode(node, newNode);
                    }
                    break;

                case FieldDeclarationSyntax node:
                    {
                        SyntaxToken token = Resolve(node.GetFirstToken());
                        SyntaxTokenList newList = node.Modifiers.Insert(0, token);

                        FieldDeclarationSyntax newNode = node.WithModifiers(newList);
                        newRoot = root.ReplaceNode(node, newNode);
                    }
                    break;
                default:
                    return null;
            }

            return document.WithSyntaxRoot(newRoot);
        }

        private static SyntaxToken Resolve(SyntaxToken firstToken, SyntaxKind defaultKind = SyntaxKind.PrivateKeyword)
        {
            SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;
            SyntaxToken token = SyntaxFactory.Token(leadingTrivia, defaultKind, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

            return token;
        }
    }
}