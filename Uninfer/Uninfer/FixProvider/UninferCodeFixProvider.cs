using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Uninfer
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UninferCodeFixProvider)), System.Composition.Shared]
	public class UninferCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				IDiagnosticDescriptorData varIsBad = new VarIsBad();
				return ImmutableArray.Create(varIsBad.Id);
			}
		}

		public override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public async override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			CancellationToken myToken = context.CancellationToken;
			SyntaxNode rootNode = await context.Document.GetSyntaxRootAsync(myToken).ConfigureAwait(false);

			TextSpan textSpan = context.Diagnostics.First().Location.SourceSpan;

			VariableDeclarationSyntax myNode = rootNode.FindNode(textSpan).Parent.ChildNodes().First() as VariableDeclarationSyntax;
			if (myNode != null)
			{
				ITypeSymbol typeSymbol = myNode.GetDeclarationTypeInfo(await context.Document.GetSemanticModelAsync(myToken).ConfigureAwait(false));

				if (typeSymbol is IErrorTypeSymbol)
					return;

				IdentifierNameSyntax varNode = myNode.ChildNodes().First() as IdentifierNameSyntax;
				if (varNode == null)
					return;

				CodeAction codeAction = CodeAction.Create("Uninferred", async token =>
				{
					SourceText sourceText = await context.Document.GetTextAsync(myToken);
					return context.Document.WithText(sourceText.Replace(varNode.Span, typeSymbol.ToString()));
				});

				context.RegisterCodeFix(codeAction, context.Diagnostics.First());
			}
		}
	}
}
