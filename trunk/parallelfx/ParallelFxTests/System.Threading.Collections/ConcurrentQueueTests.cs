// ConcurrentQueueTest.cs
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
	public class ConcurrentQueueTests
	{
		ConcurrentQueue<int> queue;
		
		[SetUpAttribute]
		public void Setup()
		{
			queue = new ConcurrentQueue<int>();
			for (int i = 0; i < 10; i++) {
				queue.Enqueue(i);
			}
		}
		
		[Test]
		public void CountTestCase()
		{
			Assert.IsTrue(queue.Count == 10, "#1");
			int value;
			queue.TryPeek(out value);
			queue.TryDequeue(out value);
			queue.TryDequeue(out value);
			Assert.IsTrue(queue.Count == 8, "#2");
			queue.Clear();
			Assert.IsTrue(queue.Count == 0, "#3");
			Assert.IsTrue(queue.IsEmpty, "#4");
		}
		
		//[Ignore]
		[Test()]
		public void EnumerateTestCase()
		{
			string s = string.Empty;
			foreach (int i in queue) {
				s += i;
			}
			Assert.IsTrue(s == "0123456789", "#1 : " + s);
		}
		
		[Test()]
		public void TryPeekTestCase()
		{
			int value;
			queue.TryPeek(out value);
			Assert.IsTrue(value == 0, "#1 : " + value);
			queue.TryDequeue(out value);
			Assert.IsTrue(value == 0, "#2 : " + value);
			queue.TryDequeue(out value);
			Assert.IsTrue(value == 1, "#3 : " + value);
			queue.TryPeek(out value);
			Assert.IsTrue(value == 2, "#4 : " + value);
			queue.TryPeek(out value);
			Assert.IsTrue(value == 2, "#5 : " + value);
		}
		
		[Test()]
		public void TryDequeueTestCase()
		{
			int value;
			queue.TryPeek(out value);
			Assert.IsTrue(value == 0, "#1");
			queue.TryDequeue(out value);
			queue.TryDequeue(out value);
			Assert.IsTrue(value == 1, "#2 : " + value);
		}
	}
}
