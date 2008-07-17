//
// Gendarme.Rules.Correctness.ProvideCorrectArgumentsToFormattingMethodsRule
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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Correctness {
	[Problem ("")]
	[Solution ("")]
	public class ProvideCorrectArgumentsToFormattingMethodsRule : Rule, IMethodRule {
		static MethodSignature formatSignature = new MethodSignature ("Format", "System.String");
		static Regex formatterRegex = new Regex ("{[0-63]((:|-| )?[a-z|A-Z]*)*}");

		private static IEnumerable<Instruction> GetCallsToStringFormat (MethodDefinition method)
		{
			return from Instruction instruction in method.Body.Instructions
			where (instruction.OpCode.FlowControl == FlowControl.Call) && 
				formatSignature.Matches ((MethodReference) instruction.Operand)	
			select instruction;
		}

		private static Instruction GetLoadStringInstruction (Instruction call)
		{
			Instruction current = call;
			while (current != null) {
				if (current.OpCode == OpCodes.Ldstr)
					return current;
				current = current.Previous;	
			}
			return null;
		}

		private static int GetExpectedParameters (string loaded)
		{
			return formatterRegex.Matches (loaded).Count;
		}

		private static int CountElementsInTheStack (MethodDefinition method, Instruction start, Instruction end)
		{
			Instruction current = start;
			int counter = 0;
			bool newarrDetected = false;
			while (end != current) {
				if (newarrDetected) {
					//Count only the stelem instructions if
					//there are a newarr instruction.
					if (current.OpCode == OpCodes.Stelem_Ref)
						counter++;
				}
				else {
					//Count with the stack
					counter += current.GetPushCount ();
					counter -= current.GetPopCount (method);
				}
				//If there are a newarr we need an special
				//behaviour
				if (current.OpCode == OpCodes.Newarr) {
					newarrDetected = true;
					counter = 0;
				}
				current = current.Next;
			}
			return counter;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			IEnumerable<Instruction> callsToStringFormat = GetCallsToStringFormat (method);	
			if (callsToStringFormat.Count () == 0)
				return RuleResult.DoesNotApply;

			foreach (Instruction call in callsToStringFormat) {
				Instruction loadString = GetLoadStringInstruction (call);
				if (loadString != null) {
					int expectedParameters = GetExpectedParameters ((string) loadString.Operand);
					//start counting skipping the ldstr
					//instruction which adds 1 to the counter
					int elementsPushed = CountElementsInTheStack (method, loadString.Next, call);
					//Console.WriteLine ("{0} ex - {1} pu- {2} method", expectedParameters, elementsPushed, method.Name);
					if (elementsPushed != expectedParameters)
						Runner.Report (method, call, Severity.Critical, Confidence.Low, String.Format ("The String.Format method is expecting {0} parameters, but only {1} are found.", expectedParameters, elementsPushed));
				}

			}
			return Runner.CurrentRuleResult;
		}
	}
}
