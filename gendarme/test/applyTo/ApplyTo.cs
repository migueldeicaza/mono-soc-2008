//
// Gendarme.Test.ApplyTo class
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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Gendarme; 
using Gendarme.Framework;
using Mono.Cecil;

namespace Gendarme.Test {
	[TestFixture]
	public abstract class ApplyTo {
		protected Settings settings;
		protected TestableRunner runner;
		protected string ConfigurationFile = "";
		static readonly string DefaultRuleset = "default";
		static readonly AssemblyDefinition FakeAssembly = AssemblyFactory.GetAssembly ("FakeAssembly.dll");
		
		[SetUp]
		public void SetUp ()
		{
			runner = new TestableRunner ();
			settings = new Settings (runner, ConfigurationFile, DefaultRuleset); 
			settings.Load ();
		}

		protected void RunGendarme ()
		{
			runner.Assemblies.Add (FakeAssembly);
			runner.Initialize ();
			runner.Run ();
		}

		protected IRule GetRule (string name)
		{
			foreach (IRule rule in runner.Rules) {
				if (String.Compare (rule.Name, name) == 0)
					return rule;
			}
			return null;
		}

		public static void CheckApplicabilityScopeFor (IEnumerable<IRule> rules, ApplicabilityScope applicabilityScope)
		{
			foreach (IRule rule in rules) 
				Assert.AreEqual (applicabilityScope, rule.ApplicabilityScope);
		}

		public virtual void TestTypeRules ()
		{
			runner.Rules.Remove (GetRule ("FakeAssemblyRule"));
			runner.Rules.Remove (GetRule ("FakeMethodRule"));
			Assert.AreEqual (1, runner.Rules.Count);
			RunGendarme ();
		}

		//Remember that test methods + constructors
		public virtual void TestMethodRules ()
		{
			runner.Rules.Remove (GetRule ("FakeAssemblyRule"));
			runner.Rules.Remove (GetRule ("FakeTypeRule"));
			Assert.AreEqual (1, runner.Rules.Count);
			RunGendarme ();
		}
	}
}
