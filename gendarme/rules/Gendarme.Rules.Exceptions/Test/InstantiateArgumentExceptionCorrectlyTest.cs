//
// Unit tests for InstantiateArgumentExceptionsCorrectlyRule
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
using Gendarme.Rules.Exceptions;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Exceptions {
	[TestFixture]
	public class InstantiateArgumentExceptionCorrectlyTest : MethodRuleTestFixture<InstantiateArgumentExceptionCorrectlyRule> {

		[Test]
		public void SkipOnBodylessMethods ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		[Test]
		public void SuccessOnEmptyMethods ()
		{
			AssertRuleSuccess (SimpleMethods.EmptyMethod);
		}

		public void ArgumentExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new ArgumentException ("Invalid parameter", "parameter");
		}

		[Test]
		public void SuccessOnArgumentExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithTwoParametersInGoodOrder");
		}

		public void ArgumentExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new ArgumentException ("parameter", "Invalid parameter");
		}

		[Test]
		public void SuccessOnArgumentExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithTwoParametersInBadOrder", 1);
		}

		public void ArgumentNullExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new ArgumentNullException ("parameter", "This parameter is null");
		}

		[Test]
		public void SuccessOnArgumentNullExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithTwoParametersInGoodOrder");
		}

		public void ArgumentNullExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new ArgumentNullException ("This parameter is null", "parameter");
		}

		[Test]
		public void FailOnArgumentNullExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithTwoParametersInBadOrder", 1);
		}

		public void ArgumentOutOfRangeExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new ArgumentOutOfRangeException ("parameter", "This parameter is out of order");
		}

		[Test]
		public void SuccessOnArgumentOutOfRangeExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithTwoParametersInGoodOrder");
		}	
		
		public void ArgumentOutOfRangeExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new ArgumentOutOfRangeException ("This parameter is out of order", "parameter");
		}

		[Test]
		public void FailOnArgumentOutOfRangeExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithTwoParametersInBadOrder", 1);
		}

		public void DuplicateWaitObjectExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new DuplicateWaitObjectException ("parameter", "A simple message");
		}

		[Test]
		public void SuccessOnDuplicateWaitObjectExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithTwoParametersInGoodOrder");
		}

		public void DuplicateWaitObjectExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new DuplicateWaitObjectException ("A simple message", "parameter");
		}

		[Test]
		public void FailOnDuplicateWaitObjectExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithTwoParametersInBadOrder", 1);
		}

		//

		public void ArgumentExceptionWithOneArgument (int parameter)
		{
			throw new ArgumentException ("parameter");
		}

		[Test]
		public void FailOnArgumentExceptionWithOneArgumentTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithOneArgument", 1);
		}

		public void ArgumentExceptionWithOneMessage (int parameter)
		{
			throw new ArgumentException ("Invalid parameter");
		}

		[Test]
		public void SuccessOnArgumentExceptionWithOneMessageTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithOneMessage");
		}

		public void ArgumentNullExceptionWithOneParameter (int parameter)
		{
			throw new ArgumentNullException ("parameter");
		}

		[Test]
		public void SuccessOnArgumentNullExceptionWithOneParameterTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithOneParameter");
		}

		public void ArgumentNullExceptionWithOneMessage (int parameter)
		{
			throw new ArgumentNullException ("This argument is null " + parameter);
		}

		[Test]
		public void FailOnArgumentNullExceptionWithOneMessageTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithOneMessage", 1);
		}

		public void ArgumentOutOfRangeExceptionWithOneParameter (int parameter)
		{
			throw new ArgumentOutOfRangeException ("parameter");
		}

		[Test]
		public void SuccessOnArgumentOutOfRangeExceptionWithOneParameterTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithOneParameter");
		}

		public void ArgumentOutOfRangeExceptionWithOneMessage (int parameter)
		{
			throw new ArgumentOutOfRangeException ("The parameter is out of range");
		}

		[Test]
		public void FailOnArgumentOutOfRangeExceptionWithOneMessageTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithOneMessage", 1);
		}

		public void DuplicateWaitObjectExceptionWithOneParameter (int parameter)
		{
			throw new DuplicateWaitObjectException ("parameter");
		}

		[Test]
		public void SuccessOnDuplicateWaitObjectExceptionWithOneParameterTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithOneParameter");
		}

		public void DuplicateWaitObjectExceptionWithOneMessage (int parameter)
		{
			throw new DuplicateWaitObjectException ("There are a duplicate wait object.");
		}

		[Test]
		public void FailOnDuplicateWaitObjectExceptionWithOneMessageTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithOneMessage", 1);
		}
	}
}
