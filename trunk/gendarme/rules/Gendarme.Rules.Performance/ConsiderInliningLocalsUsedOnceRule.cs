//
// Gendarme.Rules.Performance.ConsiderInliningLocalsUsedOnceRule
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

using System.Collections.Generic;
using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Performance {
	[Problem ("")]
	[Solution ("")]
	public class ConsiderInliningLocalsUsedOnceRule : Rule, IMethodRule {
		
		public static IDictionary<VariableDefinition, int> CountUses (MethodDefinition method)
		{
			IDictionary<VariableDefinition, int> result = new Dictionary<VariableDefinition, int> ();
			foreach (VariableDefinition variable in method.Body.Variables) 
				result.Add (variable, 0);
			
			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode == OpCodes.Ldloc_0)
					result[method.Body.Variables[0]]++;
			}

			return result;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.Body.Variables.Count == 0)
				return RuleResult.DoesNotApply;

			IDictionary<VariableDefinition, int> counted = CountUses (method);

			foreach (VariableDefinition variable in counted.Keys) {
				if (counted[variable] == 1)
					Runner.Report (method, Severity.Medium, Confidence.Low);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
