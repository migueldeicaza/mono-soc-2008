//
// Gendarme.Rules.Reliability.AvoidCallingProblematicMethodsRule class
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
using System.Linq;
using System.Collections.Generic;
using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Reliability {
	[Problem ("There are potentially dangerous calls into your code.")]
	[Solution ("You should remove or replace the call to the dangerous method.")]
	public class AvoidCallingProblematicMethodsRule : Rule, IMethodRule {
		List<string> problematicMethods = new List<string> (); 
		
		public AvoidCallingProblematicMethodsRule ()
		{
			problematicMethods.Add ("System.Void System.GC::Collect(");
			problematicMethods.Add ("System.Void System.Threading.Thread::Suspend()");
			problematicMethods.Add ("System.Void System.Threading.Thread::Resume()");
			problematicMethods.Add ("System.IntPtr System.Runtime.InteropServices.SafeHandle::DangerousGetHandle()");
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFrom(");
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFile(");
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadWithPartialName(");
		}

		private static bool IsCallInstruction (Instruction instruction)
		{
			return instruction.OpCode.FlowControl == FlowControl.Call; 
		}
		
		private static bool OperandIsNonPublic (int operand)
		{
			return (operand & 0x20) == 32;
		}

		private static bool IsAccessingWithNonPublicModifiers (Instruction call)
		{
			Instruction current = call;
			while (current != null) {
				if (current.OpCode == OpCodes.Ldc_I4_S)
					return OperandIsNonPublic (Int32.Parse (current.Operand.ToString ()));
				current = current.Previous;
			}
			return false;
		}

		private bool IsProblematicCall (Instruction call)
		{
			if (call.Operand.ToString ().StartsWith ("System.Object System.Type::InvokeMember("))
					return IsAccessingWithNonPublicModifiers (call);

			return (from dangerous in problematicMethods
				where call.Operand.ToString ().StartsWith (dangerous)
				select dangerous).Count () != 0;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) 
				if (IsCallInstruction (instruction) && IsProblematicCall (instruction))
					Runner.Report (method, instruction, Severity.Critical, Confidence.High, "You are calling a potentially dangerous method.");
				
			return Runner.CurrentRuleResult;
		}
	}
}
