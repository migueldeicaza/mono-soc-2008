//
// Unit tests for ImplementISerializableCorrectlyRule
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
using System.Runtime.Serialization;
using Gendarme.Rules.Serialization;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Serialization {
	[TestFixture]
	public class ImplementISerializableCorrectlyTest : TypeRuleTestFixture<ImplementISerializableCorrectlyRule> {

		[Serializable]
		class AutomaticSerialization {
		}

		[Serializable]
		class ImplementationWithNonSerialized : ISerializable {
			int foo;
			[NonSerialized]
			string bar;

			protected ImplementationWithNonSerialized (SerializationInfo info, StreamingContext context)
			{
				foo = info.GetInt32 ("foo");
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", foo);
			}
		}

		[Serializable]
		class ImplementationWithoutNonSerialized : ISerializable {
			int foo;
			string bar;

			protected ImplementationWithoutNonSerialized (SerializationInfo info, StreamingContext context)
			{
				foo = info.GetInt32 ("foo");
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", foo);
			}
		}

		[Test]
		public void SkipOnCanonicalScenariosTest ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}

		[Test]
		public void SkipOnAutomaticSerializationTest ()
		{
			AssertRuleDoesNotApply<AutomaticSerialization> ();
		}

		[Test]
		public void SuccessOnImplementationWithNonSerializedTest ()
		{
			AssertRuleSuccess<ImplementationWithNonSerialized> ();
		}

		[Test]
		public void FailOnImplementationWithoutNonSerializedTest ()
		{
			AssertRuleFailure<ImplementationWithoutNonSerialized> (1);
		}
	}
}
