//
// Gendarme.Rules.Design.DeclareEventHandlersCorrectlyRule
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
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;

namespace Gendarme.Rules.Design {
	[Problem ("The delegate which handles the event haven't the correct signature.")]
	[Solution ("You should correct the signature, return type or parameter types or parameter names.")]
	public class DeclareEventHandlersCorrectlyRule : Rule, ITypeRule {

		private void CheckReturnVoid (TypeDefinition current, MethodDefinition invoke)
		{
			if (String.Compare (invoke.ReturnType.ReturnType.FullName, "System.Void") != 0)
				Runner.Report (current, Severity.Medium, Confidence.High, String.Format ("The delegate should return void, not {0}", invoke.ReturnType.ReturnType.FullName));
		}

		private void CheckAmountOfParameters (TypeDefinition current, MethodDefinition invoke)
		{
			if (invoke.Parameters.Count != 2)
				Runner.Report (current, Severity.Medium, Confidence.High, String.Format ("The delegate should have 2 parameters"));
		}

		private void CheckParameterTypes (TypeDefinition current, MethodDefinition invoke)
		{
			if (invoke.Parameters.Count >= 1) {
				if (String.Compare (invoke.Parameters[0].ParameterType.FullName, "System.Object") != 0)
					Runner.Report (current, Severity.Medium, Confidence.High, String.Format ("The first parameter should have an object, not {0}", invoke.Parameters[0].ParameterType.FullName));
			}
			if (invoke.Parameters.Count >= 2) {
				if (!invoke.Parameters[1].ParameterType.Inherits ("System.EventArgs"))
					Runner.Report (current, Severity.Medium, Confidence.High, "The second parameter should be a subclass of System.EventArgs");
			}
		}

		private void CheckParameterName (TypeDefinition current, MethodDefinition invoke, int position, string expected)
		{
			if (invoke.Parameters.Count >= position + 1) {
				if (String.Compare (invoke.Parameters[position].Name, expected) != 0)
					Runner.Report (current, Severity.Low, Confidence.High, String.Format ("The expected name is {0}, not {1}", expected, invoke.Parameters[position].Name));
			}
		}

		private bool ExistsViolationsForThisRuleIn (TypeDefinition type) 
		{
			foreach (Defect defect in Runner.Defects) {
				if ((defect.Rule == this) && (defect.Location == type))
					return true;
			}
			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.Events.Count == 0)
				return RuleResult.DoesNotApply;
			
			foreach (EventDefinition each in type.Events) {
				TypeDefinition eventType = each.EventType.Resolve ();
				if (ExistsViolationsForThisRuleIn (eventType))
					continue;

				//TODO: When is in the svn, a invoke
				//MethodSignature should be created.
				MethodDefinition invoke = eventType.GetMethod ("Invoke");
				if (invoke != null) {
					//Warn the delegate type, not the client.
					CheckReturnVoid (eventType, invoke);
					CheckAmountOfParameters (eventType, invoke);
					CheckParameterTypes (eventType, invoke);
					CheckParameterName (eventType, invoke, 0, "sender");
					CheckParameterName (eventType, invoke, 1, "e");
				}
			}
							
			return Runner.CurrentRuleResult;
		}
	}
}
