// ConcurrentStackRe.cs
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
using System.Threading.Collections;
using NUnit.Framework;

namespace ParallelFxTests
{
	[TestFixture()]
	public class ConcurrentStackTests
	{
		ConcurrentStack<int> stack;
		
		[SetUpAttribute]
		public void Setup()
		{
			stack = new ConcurrentStack<int>();
			for (int i = 0; i < 10; i++) {
				stack.Push(i);
			}
		}
		
		[Test]
		public void CountTestCase()
		{
			Assert.IsTrue(stack.Count == 10, "#1");
			int value;
			stack.TryPeek(out value);
			stack.TryPop(out value);
			stack.TryPop(out value);
			Assert.IsTrue(stack.Count == 8, "#2");
			stack.Clear();
			Assert.IsTrue(stack.Count == 0, "#3");
			Assert.IsTrue(stack.IsEmpty, "#4");
		}
		
		//[Ignore]
		[Test()]
		public void EnumerateTestCase()
		{
			string s = string.Empty;
			foreach (int i in stack) {
				s += i;
			}
			Assert.IsTrue(s == "9876543210", "#1 : " + s);
		}
		
		[Test()]
		public void TryPeekTestCase()
		{
			int value;
			stack.TryPeek(out value);
			Assert.IsTrue(value == 9, "#1 : " + value);
			stack.TryPop(out value);
			Assert.IsTrue(value == 9, "#2 : " + value);
			stack.TryPop(out value);
			Assert.IsTrue(value == 8, "#3 : " + value);
			stack.TryPeek(out value);
			Assert.IsTrue(value == 7, "#4 : " + value);
			stack.TryPeek(out value);
			Assert.IsTrue(value == 7, "#5 : " + value);
		}
		
		[Test()]
		public void TryPopTestCase()
		{
			int value;
			stack.TryPeek(out value);
			Assert.IsTrue(value == 9, "#1");
			stack.TryPop(out value);
			stack.TryPop(out value);
			Assert.IsTrue(value == 8, "#2 : " + value);
		}
		
		[Test()]
		public void TryPopEmptyTestCase()
		{
			int value;
			stack.Clear();
			stack.Push(1);
			Assert.IsTrue(stack.TryPop(out value), "#1");
			Assert.IsFalse(stack.TryPop(out value), "#2");
			Assert.IsTrue(stack.IsEmpty, "#3");
		}
		
		[Test]
		public void ToArrayTest()
		{
			int[] array = stack.ToArray();
			string s = string.Empty;
			foreach (int i in array) {
				s += i;
			}
			Assert.IsTrue(s == "9876543210", "#1 : " + s);
			stack.CopyTo(array, 0);
			s = string.Empty;
			foreach (int i in array) {
				s += i;
			}
			Assert.IsTrue(s == "9876543210", "#1 : " + s);
		}
	}
}