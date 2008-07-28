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
		static string[] checkedExceptions = {
			"System.ArgumentException",
			"System.ArgumentNullException",
			"System.ArgumentOutOfRangeException",
			"System.DuplicateWaitObjectException"
			};

		public static TypeReference GetArgumentExceptionThrown (Instruction throwInstruction)
		{
			if (throwInstruction.Previous.OpCode == OpCodes.Newobj) {
				Instruction instantiation = throwInstruction.Previous;
				MethodReference method = (MethodReference) instantiation.Operand;
				if (Array.IndexOf (checkedExceptions, method.DeclaringType.FullName) != -1)
					return method.DeclaringType;
			}
			return null;
		}

		public static bool ParameterNameIsLastOperand (MethodDefinition method, Instruction throwInstruction)
		{
			int parameters = ((MethodReference) throwInstruction.Previous.Operand).Parameters.Count;
			
			Instruction current = throwInstruction;
			int counter = parameters;
			while (current != null && counter != 0) {
				if (current.OpCode == OpCodes.Ldstr) {
					string operand = (string) current.Operand;
					//The second operand, a parameter name
					if (parameters == counter) {
						bool matched = false;
						foreach (ParameterDefinition param in method.Parameters) {
							if (String.Compare (param.Name, operand) == 0) {
								matched = true;
								break;
							}
						}
						if (!matched)
							return matched;
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

		public static bool ParameterIsDescription (Instruction throwInstruction)
		{
			Instruction current = throwInstruction;

			while (current != null) {
				if (current.OpCode == OpCodes.Ldstr) {
					string operand = (string) current.Operand;
					if (!operand.Contains (" "))
						return false;
				}
				current = current.Previous;
			}
			return true;
		}

		private static bool IsArgumentException (TypeReference exceptionType)
		{
			return Array.IndexOf (checkedExceptions, exceptionType.FullName) == 0;
		}
		
		private static bool ContainsOnlyStringsAsParameters (MethodReference method)
		{
			foreach (ParameterDefinition parameter in method.Parameters) {
				if (parameter.ParameterType.FullName != "System.String")
					return false;
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
				
				TypeReference exceptionType = GetArgumentExceptionThrown (current);
				if (exceptionType == null)	
					continue;
				
				MethodReference constructor = (MethodReference) current.Previous.Operand;
				if (!ContainsOnlyStringsAsParameters (constructor))
					continue;
					
				int parameters =  constructor.Parameters.Count;
				
				if (IsArgumentException (exceptionType)) {
					if (parameters == 1 && !ParameterIsDescription (current)) {
						Runner.Report (method, current, Severity.High, Confidence.Low);
						continue;
					}
				
					if (parameters == 2 && !ParameterNameIsLastOperand (method, current))
						Runner.Report (method, current, Severity.High, Confidence.Low);
				}
				else {
					if (parameters == 1 && ParameterIsDescription (current)) {
						Runner.Report (method, current, Severity.High, Confidence.Low);
						continue;
					}
					if (parameters == 2 && ParameterNameIsLastOperand (method, current))
						Runner.Report (method, current, Severity.High, Confidence.Low);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
