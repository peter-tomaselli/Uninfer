﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Uninfer.Test
{
	[TestClass]
	public class UnitTest : CodeFixVerifier
	{
		[TestMethod]
		public void TestEmpty()
		{
			var test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void TestVarShouldBeString()
		{
			string test = @"
using System;
using System.Text;

namespace Foo
{
	class Bar
	{
		void Baz()
		{
			var qux = string.Empty;
		}
	}
}
";
			IDiagnosticDescriptorData varIsBad = new VarIsBad();
			DiagnosticResult expected = new DiagnosticResult
			{
				Id = varIsBad.Id,
				Message = string.Format(varIsBad.Format, "usage of 'var'. Type should be 'string'."),
				Severity = DiagnosticSeverity.Info,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 4) }
			};
			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestVarFix()
		{
			string test = @"
using System;
using System.Text;

namespace Foo
{
	class Bar
	{
		void Baz()
		{
			var qux = string.Empty;
		}
	}
}
";
			string testFixed = @"
using System;
using System.Text;

namespace Foo
{
	class Bar
	{
		void Baz()
		{
			string qux = string.Empty;
		}
	}
}
";
			VerifyCSharpFix(test, testFixed);
		}

		[TestMethod]
		public void TestVarIncomplete()
		{
			string test = @"
using System;
using System.Text;

namespace Foo
{
	class Bar
	{
		void Baz()
		{
			var qux = null; // not valid syntax; we should get the 'weaker' version of our diagnostic message (no inferrable type)
		}
	}
}
";
			IDiagnosticDescriptorData varIsBad = new VarIsBad();
			DiagnosticResult expected = new DiagnosticResult
			{
				Id = varIsBad.Id,
				Message = string.Format(varIsBad.Format, "usage of 'var'."),
				Severity = DiagnosticSeverity.Info,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 4) }
			};
			VerifyCSharpDiagnostic(test, expected);
		}

		[TestMethod]
		public void TestTheM()
		{
			string test = @"
using System;
using System.Text;

namespace Foo
{
	class Bar
	{
		void Baz()
		{
			string theM = ""M"";
		}
	}
}
";
			IDiagnosticDescriptorData doNotNameVariablesTheM = new DoNotNameVariablesTheM();
			DiagnosticResult expected = new DiagnosticResult
			{
				Id = doNotNameVariablesTheM.Id,
				Message = string.Format(doNotNameVariablesTheM.Format, "Do not name variables 'theM'."),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", 11, 4) }
			};
			VerifyCSharpDiagnostic(test, expected);
		}

	//	//Diagnostic and CodeFix both triggered and checked for
	//	[TestMethod]
	//	public void TestMethod2()
	//	{
	//		var test = @"
	//using System;
	//using System.Collections.Generic;
	//using System.Linq;
	//using System.Text;
	//using System.Threading.Tasks;
	//using System.Diagnostics;

	//namespace ConsoleApplication1
	//{
	//	class TypeName
	//	{
	//	}
	//}";
	//		var expected = new DiagnosticResult
	//		{
	//			Id = "Uninfer",
	//			Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
	//			Severity = DiagnosticSeverity.Warning,
	//			Locations =
	//				new[] {
	//						new DiagnosticResultLocation("Test0.cs", 11, 15)
	//					}
	//		};

	//		VerifyCSharpDiagnostic(test, expected);

	//		var fixtest = @"
	//using System;
	//using System.Collections.Generic;
	//using System.Linq;
	//using System.Text;
	//using System.Threading.Tasks;
	//using System.Diagnostics;

	//namespace ConsoleApplication1
	//{
	//	class TYPENAME
	//	{
	//	}
	//}";
	//		VerifyCSharpFix(test, fixtest);
	//	}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new UninferCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new UninferAnalyzer();
		}
	}
}
