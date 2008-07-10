//
// Gendarme.Rules.Serialization.ImplementISerializableCorrectlyRule
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
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Serialization {
	[Problem ("")]
	[Solution ("")]
	public class ImplementISerializableCorrectlyRule : Rule, ITypeRule {

		public static IList<FieldDefinition> GetFieldsUsedIn (MethodDefinition method)
		{
			IList<FieldDefinition> result = new List<FieldDefinition> ();
			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode == OpCodes.Ldfld)
					result.Add ((FieldDefinition) instruction.Operand);
			}
			return result;
		}
		
		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsSerializable || !type.Implements ("System.Runtime.Serialization.ISerializable"))
				return RuleResult.DoesNotApply;
			IList<FieldDefinition> fieldsUsed = GetFieldsUsedIn (type.GetMethod (MethodSignatures.GetObjectData));

			foreach (FieldDefinition field in type.Fields) {
				if (!fieldsUsed.Contains (field) && !field.IsNotSerialized)
					Runner.Report (type, Severity.High,
					Confidence.Low, String.Format ("The field {0} isn't going to be serialized, please use the [NonSerialized] attribute.", field.Name));
			}
			return Runner.CurrentRuleResult;
		}
	}
}
