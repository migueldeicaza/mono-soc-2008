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
using System.Linq;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;

namespace Gendarme.Rules.Design {
	[Problem ("The delegate which handles the event haven't the correct signature.")]
	[Solution ("You should correct the signature, return type or parameter types or parameter names.")]
	public class DeclareEventHandlersCorrectlyRule : Rule, ITypeRule {
		private static IEnumerable<TypeDefinition> GetEvents (TypeDefinition type)
		{
			List<TypeDefinition> result = new List<TypeDefinition> ();
			foreach (FieldDefinition field in type.Fields) {
				TypeDefinition fieldType = field.FieldType.Resolve ();
				if (fieldType.IsDelegate () && !result.Contains (fieldType))
					result.Add (fieldType);		
			}
			return result;
		}

		private static MethodDefinition GetInvokeMethod (TypeDefinition type)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (String.Compare (method.Name, "Invoke") == 0)
					return method;
			}
			return null;
		}

		private void CheckReturnVoid (TypeDefinition type, MethodDefinition invoke)
		{
			if (String.Compare (invoke.ReturnType.ReturnType.FullName, "System.Void") != 0)
				Runner.Report (type, Severity.Medium, Confidence.High, String.Format ("The delegate should return void, not {0}", invoke.ReturnType.ReturnType.FullName));
		}

		private void CheckParameterTypes (TypeDefinition type, MethodDefinition invoke)
		{
			if (invoke.Parameters.Count != 2) {
				Runner.Report (type, Severity.Low, Confidence.High, String.Format ("The signature should have 2 parameters and it has {0} parameters", invoke.Parameters.Count));
				return;
			}
			
			if (String.Compare (invoke.Parameters[0].ParameterType.FullName, "System.Object") != 0)
				Runner.Report (type, Severity.Medium, Confidence.High, String.Format ("The first parameter should have a object, not {0}", invoke.Parameters[0].ParameterType.FullName));

			if (!invoke.Parameters[1].ParameterType.Inherits ("System.EventArgs"))
				Runner.Report (type, Severity.Medium, Confidence.High, "The second parameter should be a subclass of System.EventArgs");
		}

		private void CheckParameterName (TypeDefinition type, MethodDefinition invoke, int position, string expected)
		{
			if (invoke.Parameters.Count >= position + 1) {
				if (String.Compare (invoke.Parameters[position].Name, expected) != 0)
					Runner.Report (type, Severity.Low, Confidence.High, String.Format ("The expected name is {0}, not {1}", expected, invoke.Parameters[position].Name));
			}
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			IEnumerable<TypeDefinition> events = GetEvents (type);
			if (events.Count () == 0)
				return RuleResult.DoesNotApply;
			
			foreach (TypeDefinition eventType in events) {
				MethodDefinition invoke = GetInvokeMethod (eventType);
				if (invoke != null) {
					CheckReturnVoid (eventType, invoke);
					CheckParameterTypes (eventType, invoke);
					CheckParameterName (eventType, invoke, 0, "sender");
					CheckParameterName (eventType, invoke, 1, "e");
				}
			}
				
			return Runner.CurrentRuleResult;
		}
	}
}
