//
// Gendarme.Rules.Serialization.CallBaseMethodsOnISerializableTypesRule
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
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Serialization {
	[Problem ("")]
	[Solution ("")]
	public class CallBaseMethodsOnISerializableTypesRule : Rule, ITypeRule {

		private static bool InheritsFromISerializableImplementation (TypeReference type)
		{
			TypeDefinition current = type.Resolve ();
			if (current == null || current.FullName == "System.Object")
				return false;
			if (current.IsSerializable && current.Implements ("System.Runtime.Serialization.ISerializable"))
				return true;

			return InheritsFromISerializableImplementation (current.BaseType);
		}

		private string[] ExtractCallInformation (string operand)
		{
			string[] firstSplit = operand.Split (' ');
			string[] secondSplit = firstSplit[1].Replace ("::", ":").Split (':');
			return new string[] {firstSplit[0], secondSplit[0], secondSplit[1]};
		}

		private void CheckCallingBaseMethod (TypeDefinition type, MethodSignature methodSignature)
		{
			MethodDefinition method = type.GetMethod (methodSignature);
			if (method == null)
				return; // Perhaps should report that doesn't exist the method (only with ctor).
			bool foundBaseCall = false;
			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl == FlowControl.Call) {
					string[] callInformation = ExtractCallInformation (instruction.Operand.ToString ());
					foundBaseCall |=  String.Compare (type.BaseType.FullName, callInformation[1]) == 0 &&
						String.Compare (methodSignature.ReturnType, callInformation[0]) == 0 &&
						callInformation [2].StartsWith (methodSignature.Name);
				}
			}

			if (!foundBaseCall)
				Runner.Report (method, Severity.High, Confidence.Low);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!InheritsFromISerializableImplementation (type.BaseType))
				return RuleResult.DoesNotApply;
			
			CheckCallingBaseMethod (type, MethodSignatures.SerializationConstructor);
			CheckCallingBaseMethod (type, MethodSignatures.GetObjectData);

			return Runner.CurrentRuleResult;
		}
	}
}
