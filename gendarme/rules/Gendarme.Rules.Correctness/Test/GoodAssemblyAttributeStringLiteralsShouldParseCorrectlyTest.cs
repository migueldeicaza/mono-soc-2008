//
// Unit tests for AttributeStringLiteralShouldParseCorrectlyRule
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

using System.Reflection;
using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using NUnit.Framework;
using Test.Rules.Correctness;
using Test.Rules.Helpers;
using Mono.Cecil;

[assembly: ValidSince ("1.0.0.0")]
[assembly: Reference ("http://www.mono-project.com/Gendarme")]
[assembly: Uses ("00000101-0000-0000-c000-000000000046")]

namespace Test.Rules.Correctness {
	[TestFixture]
	public class GoodAssemblyAttributeStringLiteralsShouldParseCorrectlyTest {
		private IAssemblyRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;
		
		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			runner = new TestRunner (new AttributeStringLiteralsShouldParseCorrectlyRule ());
		}

		[Test]
		public void SuccessOnThisAssembly ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckAssembly (assembly));
			Assert.AreEqual (0, runner.Defects.Count);
		}


	}
}