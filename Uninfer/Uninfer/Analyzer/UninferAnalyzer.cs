using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Uninfer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class UninferAnalyzer : DiagnosticAnalyzer
	{
		#region DiagnosticAnalyzer

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(DoNotNameVariablesTheM.BuiltDescriptor(), VarIsBad.BuiltDescriptor());
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.VariableDeclaration);
		}

		#endregion

		private readonly IVariableDeclarationSyntaxDiagnosticData DoNotNameVariablesTheM = new DoNotNameVariablesTheM();

		private readonly IVariableDeclarationSyntaxDiagnosticData VarIsBad = new VarIsBad();

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			VariableDeclarationSyntax myNode = context.Node as VariableDeclarationSyntax;
			if (myNode == null)
				return;

			DoNotNameVariablesTheM.AnalyzeNode(myNode, context);

			VarIsBad.AnalyzeNode(myNode, context);
		}

		private void Junk()
		{
			var foo = string.Empty;
		}
	}

	interface IDiagnosticDescriptorData
	{
		string Category { get; }

		DiagnosticSeverity DefaultSeverity { get; }

		string Format { get; }

		string Id { get; }

		bool IsEnabledByDefault { get; }

		string Title { get; }
	}

	interface ISyntaxNodeAnalyzerDelegate<T>
		where T : SyntaxNode
	{
		void AnalyzeNode(T node, SyntaxNodeAnalysisContext context);
	}

	interface IVariableDeclarationSyntaxDiagnosticData : IDiagnosticDescriptorData, ISyntaxNodeAnalyzerDelegate<VariableDeclarationSyntax> { }

	static class DiagnosticDescriptorDataExtensions
	{
		internal static DiagnosticDescriptor BuiltDescriptor(this IDiagnosticDescriptorData descriptorData)
		{
			return new DiagnosticDescriptor(descriptorData.Id, descriptorData.Title, descriptorData.Format, descriptorData.Category, descriptorData.DefaultSeverity, descriptorData.IsEnabledByDefault);
		}
	}

	struct DoNotNameVariablesTheM : IVariableDeclarationSyntaxDiagnosticData
	{
		public string Category { get { return ""; } }

		public DiagnosticSeverity DefaultSeverity { get { return DiagnosticSeverity.Warning; } }

		public string Format { get { return "Hey! {0}"; } }

		public string Id { get { return "UIF002"; } }

		public bool IsEnabledByDefault { get { return true; } }

		public string Title { get { return "theM"; } }

		public void AnalyzeNode(VariableDeclarationSyntax node, SyntaxNodeAnalysisContext context)
		{
			if (node.Variables.Any(v => v.Identifier.Text == "theM"))
			{
				Diagnostic diagnostic = Diagnostic.Create(this.BuiltDescriptor(), node.GetLocation(), "Do not name variables 'theM'.");
				context.ReportDiagnostic(diagnostic);
			}
		}
	}

	struct VarIsBad : IVariableDeclarationSyntaxDiagnosticData
	{
		public string Category { get { return ""; } }

		public DiagnosticSeverity DefaultSeverity { get { return DiagnosticSeverity.Info; } }

		public string Format { get { return "Info: {0}"; } }

		public string Id { get { return "UIF001"; } }

		public bool IsEnabledByDefault { get { return true; } }

		public string Title { get { return "Uninferred"; } }

		public void AnalyzeNode(VariableDeclarationSyntax node, SyntaxNodeAnalysisContext context)
		{
			if (node.Type.IsVar)
			{
				ITypeSymbol type = node.GetDeclarationTypeInfo(context.SemanticModel);
				string message = string.Empty;
				if (type is IErrorTypeSymbol)
				{
					message = "usage of 'var'.";
				}
				else
				{
					message = string.Format("usage of 'var'. Type should be '{0}'.", type.ToString());
				}
				Diagnostic diagnostic = Diagnostic.Create(this.BuiltDescriptor(), node.GetLocation(), message);
				context.ReportDiagnostic(diagnostic);
			}
		}
	}

	static class VariableDeclarationSyntaxExtensions
	{
		internal static ITypeSymbol GetDeclarationTypeInfo(this VariableDeclarationSyntax syntax, SemanticModel semanticModel)
		{
			ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(syntax.ChildNodes().First()).Type;
			/* er... hello */
			return typeSymbol;
		}
	}
}
