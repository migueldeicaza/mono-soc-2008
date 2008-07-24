// TaskTest.cs
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
using System.Threading.Tasks;

using NUnit.Framework;
using NUnit.Mocks;

namespace ParallelFxTests
{
	[TestFixture()]
	public class TaskTest
	{
		Task[]      tasks;
		//IScheduler  mockScheduler;
		//DynamicMock mock;
		static readonly int max = 3 * Environment.ProcessorCount;
		const int testRun = 10;
		
		[SetUp]
		public void Setup()
		{
			/*mock = new DynamicMock(typeof(IScheduler));
			mockScheduler = (IScheduler)mock.MockInstance;*/
			tasks = new Task[max];
			//TaskManager.Current = new TaskManager(new TaskManagerPolicy(), mockScheduler);
			
		}
		
		void InitWithDelegate(Action<object> action)
		{
			for (int i = 0; i < max; i++) {
				tasks[i] = Task.Create(action);
			}
		}
		
		void InitWithDelegate(Action<object> action, int startIndex)
		{
			for (int i = startIndex; i < max; i++) {
				tasks[i] = Task.Create(action);
			}
		}
		
		[TestAttribute]
		public void WaitAnyTest()
		{
			int achieved = 0;
			tasks[0] = Task.Create(delegate {
				Interlocked.Increment(ref achieved);
			});
			InitWithDelegate(delegate {
				Thread.Sleep(1000);
				Interlocked.Increment(ref achieved);
			}, 1);
			int index = Task.WaitAny(tasks);
			Assert.AreNotEqual(0, achieved, "#1");
			Assert.Less(index, max, "#3");
			Assert.GreaterOrEqual(index, 0, "#2");
		}
		
		[TestAttribute]
		public void WaitAllTest()
		{
			int achieved = 0;
			InitWithDelegate(delegate { Interlocked.Increment(ref achieved); });
			Task.WaitAll(tasks);
			Assert.AreEqual(max, achieved, "#1");
		}
	}
}
