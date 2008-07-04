//
// Gendarme.Rules.Serialization.MarkAllNonSerializableFieldsRule
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
using Mono.Cecil;

namespace Gendarme.Rules.Serialization {
	[Problem ("This type is Serializable, but contains fields that aren't serializable and this can drive you to some troubles and SerializationExceptions.")]
	[Solution ("Make sure you are marking all non serializable fields with the NonSerialized attribute, or implement your custom serialization.")]
	public class MarkAllNonSerializableFieldsRule : Rule, ITypeRule {
		const string ISerializable = "System.Runtime.Serialization.ISerializable";
		const string MessageError = "The field {0} isn't serializable.";
		const string InterfaceMessageError = "Serialization of interface {0} as field {1} unknown until runtime";

		public RuleResult CheckType (TypeDefinition type)
		{
			//if isn't serializable or
			//implements a custom serialization
			if (!type.IsSerializable || type.Implements (ISerializable))
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsNotSerialized) { 
					TypeDefinition fieldType = field.FieldType.Resolve ();

					if (fieldType.IsInterface) {
						Runner.Report (field, Severity.Critical, Confidence.Low, String.Format (InterfaceMessageError, fieldType, field.Name));
						continue;
					}
					if (!fieldType.IsEnum && !fieldType.IsSerializable)
						Runner.Report (field, Severity.Critical, Confidence.High, String.Format (MessageError, field.Name));
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
