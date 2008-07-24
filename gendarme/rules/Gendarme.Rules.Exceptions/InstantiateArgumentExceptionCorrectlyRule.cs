//
// Gendarme.Rules.Exceptions.InstantiateArgumentExceptionCorrectlyRule
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
using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {
	[Problem ("")]
	[Solution ("")]
	public class InstantiateArgumentExceptionCorrectlyRule : Rule, IMethodRule {

		public static bool IsThrowingArgumentException (Instruction throwInstruction)
		{
			if (throwInstruction.Previous.OpCode == OpCodes.Newobj) {
				Instruction instantiation = throwInstruction.Previous;
				MethodReference method = (MethodReference) instantiation.Operand;
				return String.Compare (method.DeclaringType.FullName, "System.ArgumentException") == 0;
			}
			return false;
		}

		public static bool OperandsAreInCorrectOrder (MethodDefinition method, Instruction throwInstruction)
		{
			int parameters = ((MethodReference) throwInstruction.Previous.Operand).Parameters.Count;
			
			Instruction current = throwInstruction;
			int counter = parameters;
			while (current != null && counter != 0) {
				if (current.OpCode == OpCodes.Ldstr) {
					string operand = (string) current.Operand;
					//The second operand, a parameter name
					//(without space)
					if (parameters == counter) {
						if (operand.Contains (" "))
							return false;
					}
					//The first operand, would be handy to
					//have a description
					else {
						if (!operand.Contains (" "))
							return false;
					}
					counter--;
				}
				current = current.Previous;
			}
			
			return true;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction current in method.Body.Instructions) {
				if (current.OpCode != OpCodes.Throw) 
					continue;
				
				if (!IsThrowingArgumentException (current))	
					continue;

				if (!OperandsAreInCorrectOrder (method, current))
					Runner.Report (method, current, Severity.High, Confidence.Low);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
