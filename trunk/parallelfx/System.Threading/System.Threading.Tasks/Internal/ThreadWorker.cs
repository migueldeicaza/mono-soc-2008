// ThreadWorker.cs
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
using System.Threading.Collections;

namespace System.Threading.Tasks
{
	internal class ThreadWorker
	{
		readonly Thread workerThread;
		readonly ThreadWorker[] others;
		
		readonly DynamicDeque<ThreadStart>    dDeque;
		readonly ConcurrentStack<ThreadStart> sharedWorkQueue;
		
		// Flag to tell if workerThread is running
		int started = 0; 
		readonly bool isLocal;
		readonly int workerLength;
		
		const int maxRetry = 5;
		
		public ThreadWorker(ThreadWorker[] others, ConcurrentStack<ThreadStart> sharedWorkQueue):
			this(others, sharedWorkQueue, true)
		{
			
		}
		
		public ThreadWorker(ThreadWorker[] others, ConcurrentStack<ThreadStart> sharedWorkQueue, bool createThread)
		{
			if (createThread) {
				this.workerThread = new Thread(new ThreadStart(WorkerMethod));
				//this.workerThread.IsBackground = true;
			} else {
				this.workerThread = Thread.CurrentThread;
				isLocal = true;
			}
			this.others = others;
			this.workerLength = others.Length;
			this.dDeque = new DynamicDeque<System.Threading.ThreadStart>();
			this.sharedWorkQueue = sharedWorkQueue;
		}
		
		public void Pulse()
		{
			// If the thread was stopped then set it in use and restart it
			int result = Interlocked.Exchange(ref started, 1);
			if (result != 0)
				return;
			if (!isLocal)
				workerThread.Start();
		}
		
		internal void WorkerMethod()
		{
			/* if sharedWorkQueue is not empty then repeatdly
			* Dequeue from it until there is nothing else in it and add it do dDeque (other workers do the same on their thread)
			* when it's empty do all the work in the local dDeque (other worker might steal from us). When our deque is empty 
			* Steal from others by PopTop them and PushBottom the result to our deque. When it's done go back to the beginning.
			* If every thing is empty then let the thread's method die. It can be started again by the Scheduler with Pulse.
			*/
			//PopResult result = PopResult.Succeed;
			while (!sharedWorkQueue.IsEmpty) {
				// We fill up our work deque concurrently with other ThreadWorker	
				ThreadStart value;
				while (sharedWorkQueue.TryPop(out value))
					dDeque.PushBottom(value);
				// Now we process our work
				while (dDeque.PopBottom(out value) == PopResult.Succeed)
					value();
				// When we have finished, steal from other worker
				ThreadWorker other;
				// Repeat the operation a little so that we can let other things process.
				for (int j = 0; j < maxRetry; j++) {
					for (int i = 0; i < workerLength; i++) {
						if ((other = others[i]) == null || other == this)
							continue;
						while (other.dDeque.PopTop(out value) == PopResult.Succeed) {
							value();	
						}
					}
				}
			}
			// If there is no more work, finish the method
			// Just before the method dies, set the start flag
			started = 0;
			//Console.WriteLine("End participation of " + this.workerThread.ManagedThreadId + " because of " + result.ToString());
		}
		
		// Almost same as above but with an added predicate and treating one item at a time. 
		// It's used by Scheduler Participate(...) method for special waiting case like
		// Task.WaitAll(someTasks) or Task.WaitAny(someTasks)
		internal void WorkerMethod(Func<bool> predicate)
		{	
			while (!predicate()) {
				ThreadStart value;
				
				if (!sharedWorkQueue.IsEmpty) {
					// Dequeue only one item as we have restriction
					if (sharedWorkQueue.TryPop(out value))
						value();
					// First check to see if we comply to predicate
					if (predicate()) {
						return;
					}
				}
				
				// Try to complete other worker work since our desired tasks may be there
				ThreadWorker other;
				for (int i = 0; i < workerLength; i++) {
					if ((other = others[i]) == null || other == this)
						continue;
					if (other.dDeque.PopTop(out value) == PopResult.Succeed) {
						value();
					}
					if (predicate()) {
						return;
					}
				}
			}
		}
		
		public bool Equals(ThreadWorker other)
		{
			return (other == null) ? false : this.workerThread.ManagedThreadId == other.workerThread.ManagedThreadId;	
		}
		
		public override bool Equals (object obj)
		{
			ThreadWorker temp = obj as ThreadWorker;
			return temp == null ? false : Equals(temp);
		}
		
		public override int GetHashCode ()
		{
			return workerThread.ManagedThreadId.GetHashCode();
		}
	}
}
