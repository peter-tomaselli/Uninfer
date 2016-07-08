using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

		private readonly DiagnosticDescriptorData DoNotNameVariablesTheM = new DoNotNameVariablesTheM();

		private readonly DiagnosticDescriptorData VarIsBad = new VarIsBad();

		private void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			VariableDeclarationSyntax myNode = context.Node as VariableDeclarationSyntax;
			if (myNode == null)
				return;

			if (myNode.Type.IsVar)
			{
				// exterminate
				Diagnostic diagnostic = Diagnostic.Create(VarIsBad.BuiltDescriptor(), myNode.GetLocation(), "`var` is bad.");
				context.ReportDiagnostic(diagnostic);
			}

			if (myNode.Variables.Any(v => v.Identifier.Text == "theM"))
			{
				Diagnostic diagnostic = Diagnostic.Create(DoNotNameVariablesTheM.BuiltDescriptor(), myNode.GetLocation(), "Do not name variables `theM`.");
				context.ReportDiagnostic(diagnostic);
			}
		}

		private void Junk()
		{
			var foo = "foo";
		}
	}

	interface DiagnosticDescriptorData
	{
		string Category { get; }

		DiagnosticSeverity DefaultSeverity { get; }

		string Format { get; }

		string Id { get; }

		bool IsEnabledByDefault { get; }

		string Title { get; }
	}

	static class DiagnosticDescriptorDataExtensions
	{
		internal static DiagnosticDescriptor BuiltDescriptor(this DiagnosticDescriptorData descriptorData)
		{
			return new DiagnosticDescriptor(descriptorData.Id, descriptorData.Title, descriptorData.Format, descriptorData.Category, descriptorData.DefaultSeverity, descriptorData.IsEnabledByDefault);
		}
	}

	struct DoNotNameVariablesTheM : DiagnosticDescriptorData
	{
		public string Category { get { return ""; } }

		public DiagnosticSeverity DefaultSeverity { get { return DiagnosticSeverity.Warning; } }

		public string Format { get { return "Hey! {0}"; } }

		public string Id { get { return "UIF002"; } }

		public bool IsEnabledByDefault { get { return true; } }

		public string Title { get { return "theM"; } }
	}

	struct VarIsBad : DiagnosticDescriptorData
	{
		public string Category { get { return ""; } }

		public DiagnosticSeverity DefaultSeverity { get { return DiagnosticSeverity.Info; } }

		public string Format { get { return "Hey! {0}"; } }

		public string Id { get { return "UIF001"; } }

		public bool IsEnabledByDefault { get { return true; } }

		public string Title { get { return "Uninferred"; } }
	}
}
