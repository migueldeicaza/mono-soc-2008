//
// Fake Objects for Testing stuff
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;

namespace FakeRules {
	public class FakeAssemblyRule : Rule, IAssemblyRule {
		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			Runner.Report (assembly, Severity.High, Confidence.High);
			return RuleResult.Failure;
		}
	}

	public class FakeMethodRule : Rule, IMethodRule {
		public RuleResult CheckMethod (MethodDefinition method)
		{
			Runner.Report (method, Severity.High, Confidence.High);
			return RuleResult.Failure;
		}
	}

	public class FakeTypeRule : Rule, ITypeRule {
		public RuleResult CheckType (TypeDefinition type) 
		{
			//Skip the <Module> type declaration.
			if (type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;
			Runner.Report (type, Severity.High, Confidence.High);
			return RuleResult.Failure;
		}
	}
}
