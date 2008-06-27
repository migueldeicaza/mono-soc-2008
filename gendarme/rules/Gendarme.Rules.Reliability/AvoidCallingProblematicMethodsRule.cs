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
using System.Reflection;
using System.Collections.Generic;
using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Reliability {
	[Problem ("There are potentially dangerous calls into your code.")]
	[Solution ("You should remove or replace the call to the dangerous method.")]
	public class AvoidCallingProblematicMethodsRule : Rule, IMethodRule {
		Dictionary<string, ProblematicMethodInfo> problematicMethods = new Dictionary<string, ProblematicMethodInfo> (); 
	
		private struct ProblematicMethodInfo {
			Severity severity;
			Predicate<Instruction> predicate;

			public ProblematicMethodInfo (Severity severity, Predicate<Instruction> predicate)
			{
				this.severity = severity;
				this.predicate = predicate;
			}
			
			public Severity Severity {
				get {
					return severity;
				}
			}

			public Predicate<Instruction> Predicate {
				get {
					return predicate;
				}
			}
		}

		public AvoidCallingProblematicMethodsRule ()
		{
			problematicMethods.Add ("System.Void System.GC::Collect(", 
				new ProblematicMethodInfo (Severity.Critical, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Void System.Threading.Thread::Suspend()", 
				new ProblematicMethodInfo (Severity.Medium, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Void System.Threading.Thread::Resume()", 
				new ProblematicMethodInfo (Severity.Medium, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.IntPtr System.Runtime.InteropServices.SafeHandle::DangerousGetHandle()", 
				new ProblematicMethodInfo (Severity.Critical, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFrom(", 
				new ProblematicMethodInfo (Severity.High, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadFile(", 
				new ProblematicMethodInfo (Severity.High, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Reflection.Assembly System.Reflection.Assembly::LoadWithPartialName(", 
				new ProblematicMethodInfo (Severity.High, 
					delegate (Instruction call) {return true;}));
			problematicMethods.Add ("System.Object System.Type::InvokeMember(", 
				new ProblematicMethodInfo (Severity.Critical, 
					delegate (Instruction call) {return IsAccessingWithNonPublicModifiers (call);}));
		}

		private static bool OperandIsNonPublic (int operand)
		{
			return ((BindingFlags) operand & BindingFlags.NonPublic) == BindingFlags.NonPublic;
		}

		private static bool IsAccessingWithNonPublicModifiers (Instruction call)
		{
			Instruction current = call;
			while (current != null) {
				if (current.OpCode == OpCodes.Ldc_I4_S || current.OpCode == OpCodes.Ldc_I4)
					return OperandIsNonPublic ((int) (sbyte) current.Operand);
				current = current.Previous;
			}
			return false;
		}

		private Severity? IsProblematicCall (Instruction call)
		{
			string operand = call.Operand.ToString ();

			var query = from dangerous in problematicMethods.Keys
				where operand.StartsWith (dangerous)
				select dangerous;

			if (query.Count () != 0) 
				if (problematicMethods [query.First ()].Predicate (call))
					return problematicMethods[query.First ()].Severity;
			return null;
		}
		
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl == FlowControl.Call){
					Severity? severity = IsProblematicCall (instruction);
					if (severity.HasValue) 
						Runner.Report (method, instruction, severity.Value, Confidence.High, "You are calling a potentially dangerous method.");
				}
			}	

			return Runner.CurrentRuleResult;
		}
	}
}
