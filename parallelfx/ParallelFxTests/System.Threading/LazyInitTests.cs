// LazyInitTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Threading;

using NUnit;
using NUnit.Core;
using NUnit.Framework;

namespace ParallelFxTests
{
	[TestFixtureAttribute]
	public class LazyInitTests
	{
		LazyInit<int> val;
		
		[TestAttribute]
		public void AllowMultipleExecutionTestCase()
		{
			val = new LazyInit<int>(() => 1, LazyInitMode.AllowMultipleExecution);
			AssertLazyInit(val);
		}
		
		[TestAttribute]
		public void EnsureSingleExecutionTestCase()
		{
			val = new LazyInit<int>(() => 1, LazyInitMode.EnsureSingleExecution);
			AssertLazyInit(val);
		}
		
		[TestAttribute]
		public void ThreadLocalTestCase()
		{
			val = new LazyInit<int>(() => 1, LazyInitMode.ThreadLocal);
			AssertLazyInit(val);
		}
		
		void AssertLazyInit(LazyInit<int> value)
		{
			Assert.IsFalse(value.IsInitialized, "#1");
			Assert.AreEqual(1, value.Value, "#2");
			Assert.IsTrue(value.IsInitialized, "#3");
			Assert.AreEqual(value, value, "#4");
			Assert.AreEqual(1.ToString(), value.ToString(), "#5");
			Assert.AreEqual(1.GetHashCode(), value.GetHashCode(), "#6");
		}
	}
}
