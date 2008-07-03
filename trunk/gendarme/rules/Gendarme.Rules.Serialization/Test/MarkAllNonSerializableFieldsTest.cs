//
// Unit test for Gendarme.Rules.Serialization.MarkAllNonSerializableFieldsRule
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
using Gendarme.Rules.Serialization;
using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Serialization {
	[TestFixture]
	public class MarkAllNonSerializableFieldsTest : TypeRuleTestFixture<MarkAllNonSerializableFieldsRule> {
		
		class NonSerializableClass {
		}

		[Serializable]
		class SerializableClass {
		}

		[Serializable]
		class SerializableWithoutMarks {
			NonSerializableClass nonSerializableClass;
			NonSerializableClass nonSerializableClass1;
		}

		[Serializable]
		class SerializableWithMarks {
			[NonSerialized]
			NonSerializableClass nonSerializableClass;
		}

		[Serializable]
		class SerializableWithSerializableFields {
			SerializableClass serializableClass;
		}

		[Test]
		public void SkipOnNonSerializableClassesTest ()
		{
			AssertRuleDoesNotApply<NonSerializableClass> ();
		}

		[Test]
		public void FailOnSerializableClassWithoutMarksTest ()
		{
			AssertRuleFailure<SerializableWithoutMarks> (2);
		}

		[Test]
		public void SuccessOnSerializableClassWithMarksTest ()
		{
			AssertRuleSuccess<SerializableWithMarks> ();
		}

		[Test]
		public void SuccessOnSerializableClassWithSerializableFieldsTest ()
		{
			AssertRuleSuccess<SerializableWithSerializableFields> ();
		}
	}
}