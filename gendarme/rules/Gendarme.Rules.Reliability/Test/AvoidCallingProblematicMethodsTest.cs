//
// Unit test for Gendarme.Rules.Reliability.AvoidCallingProblematicMethodsRule 
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Gendarme.Rules.Reliability;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Reliability {
	[TestFixture]
	public class AvoidCallingProblematicMethodsTest : MethodRuleTestFixture<AvoidCallingProblematicMethodsRule> {

		[Test]
		public void MethodsWithoutBodyTest () 
		{
			AssertRuleDoesNotApply<SimpleMethods> ("strncpy");
		}

		[Test]
		public void EmptyMethodsTest ()
		{
			AssertRuleSuccess<SimpleMethods> ("DoNothing");
		}

		public void MethodWithGCCall ()
		{
			List<string> list = new List<string> ();
			list.Add ("foo");
			list.Add ("bar");
			list = null;
			GC.Collect ();
		}

		[Test]
		public void MethodWithGCCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithGCCall");
		}

		public void MethodWithThreadSuspendCall ()
		{
			Thread thread = new Thread (delegate () {
				Console.WriteLine ("Stupid code");
			});

			thread.Suspend ();
		}

		[Test]
		public void MethodWithThreadSuspendCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithThreadSuspendCall");
		}

		public void MethodWithThreadResumeCall ()
		{
			Thread thread = new Thread (delegate () {
				Console.WriteLine ("Useless code");
			});

			thread.Resume ();
		}

		[Test]
		public void MethodWithThreadResumeCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithThreadResumeCall");
		}

		public void MethodWithInvokeMemberWithPrivateFlagsCall ()
		{
			this.GetType ().InvokeMember ("Foo", BindingFlags.NonPublic, null, null, Type.EmptyTypes);
		}

		[Test]
		public void MethodWithInvokeMemberWithPrivateFlagsCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithInvokeMemberWithPrivateFlagsCall");
		}

		public void MethodWithInvokeMemberWithoutPrivateFlagsCall ()
		{
			this.GetType ().InvokeMember ("Foo", BindingFlags.Public, null, null, Type.EmptyTypes);
		}

		[Test]
		public void MethodWithInvokeMemberWithoutPrivateFlagsCallTest ()
		{
			AssertRuleSuccess<AvoidCallingProblematicMethodsTest> ("MethodWithInvokeMemberWithoutPrivateFlagsCall");
		}
		
		private class MySafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
			public MySafeHandle () : base (true)
			{
			}

			protected override bool ReleaseHandle () 
			{
				return true;
			}
		}

		public void MethodWithSafeHandleDangerousGetHandleCall ()
		{
			MySafeHandle myHandle = new MySafeHandle ();
			IntPtr handlePtr = myHandle.DangerousGetHandle ();
		}

		[Test]
		public void MethodWithSafeHandleDangerousGetHandleCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithSafeHandleDangerousGetHandleCall");
		}

		public void MethodWithAssemblyLoadFromCall ()
		{
			Assembly myAssembly = Assembly.LoadFrom ("myAssembly.dll");	
		}

		[Test]
		public void MethodWithAssemblyLoadFromCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithAssemblyLoadFromCall");
		}

		public void MethodWithAssemblyLoadFileCall ()
		{
			Assembly myAssembly = Assembly.LoadFile ("myAssembly.dll");
		}

		[Test]
		public void MethodWithAssemblyLoadFileCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithAssemblyLoadFileCall");
		}

		public void MethodWithAssemblyLoadWithPartialNameCall ()
		{
			Assembly myAssembly = Assembly.LoadFile ("MyAssembly");
		}

		[Test]
		public void MethodWithAssemblyLoadWithPartialNameCallTest ()
		{
			AssertRuleFailure<AvoidCallingProblematicMethodsTest> ("MethodWithAssemblyLoadWithPartialNameCall");
		}

	}
}
