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
		Thread workerThread;
		readonly ThreadWorker[] others;
		
		readonly DynamicDeque<Task>    dDeque;
		//readonly OptimizedStack<Task> sharedWorkQueue;
		readonly ConcurrentStack<Task> sharedWorkQueue;
		
		// Flag to tell if workerThread is running
		int started = 0; 
		readonly bool isLocal;
		readonly int workerLength;
		readonly int stealingStart;
		
		const int maxRetry = 5;
		const int sleepTimeBeforeRetry = 50;
		
		//public ThreadWorker(ThreadWorker[] others, OptimizedStack<Task> sharedWorkQueue,
		public ThreadWorker(ThreadWorker[] others, ConcurrentStack<Task> sharedWorkQueue,
		                    int maxStackSize, ThreadPriority priority):
			this(others, sharedWorkQueue, true, maxStackSize, priority)
		{
			
		}
		
		//public ThreadWorker(ThreadWorker[] others, OptimizedStack<Task> sharedWorkQueue,
		public ThreadWorker(ThreadWorker[] others, ConcurrentStack<Task> sharedWorkQueue,
		                    bool createThread, int maxStackSize, ThreadPriority priority)
		{
			this.others = others;
			this.dDeque = new DynamicDeque<Task>();
			this.sharedWorkQueue = sharedWorkQueue;
			
			this.workerLength = others.Length;
			this.stealingStart= (Array.FindIndex(others, delegate (ThreadWorker w) {
				return w == this; }) + 1) % workerLength; 
			
			if (createThread) {
				ThreadStart func = new ThreadStart(delegate {
					while (started == 1) {
						WorkerMethod();
						Thread.Sleep(sleepTimeBeforeRetry);
					}
					// If there is no more work, finish the method
					// Just before the method dies, set the start flag
					started = 0;
				});
				if (maxStackSize == 0)
					this.workerThread = new Thread(func);
				else
					this.workerThread = new Thread(func, maxStackSize);
				this.workerThread.IsBackground = true;
				this.workerThread.Priority = priority;
			} else {
				this.workerThread = Thread.CurrentThread;
				isLocal = true;
			}
		}
		
		public void Pulse()
		{
			// If the thread was stopped then set it in use and restart it
			int result = Interlocked.Exchange(ref started, 1);
			if (result != 0)
				return;
			if (!isLocal) {
				if (this.workerThread.ThreadState != ThreadState.Unstarted) {
					this.workerThread = new Thread(new ThreadStart(WorkerMethod));
					this.workerThread.IsBackground = true;
				}
				workerThread.Start();
			}
		}
		
		public void Stop()
		{
			// Set the flag to stop so that the while in the thread will stop
			// doing its infinite loop.
			started = 0;
		}
		
		internal void WorkerMethod()
		{
			/* if sharedWorkQueue is not empty then repeatdly
			* Dequeue from it until there is nothing else in it and add it do dDeque (other workers do the same on their thread)
			* when it's empty do all the work in the local dDeque (other worker might steal from us). When our deque is empty 
			* Steal from others by PopTop them and PushBottom the result to our deque. When it's done go back to the beginning.
			* If every thing is empty then let the thread's method die. It can be started again by the Scheduler with Pulse.
			*/
			do {
				Task value;
				// We fill up our work deque concurrently with other ThreadWorker
				while (!sharedWorkQueue.IsEmpty) {
					while (sharedWorkQueue.TryPop(out value))
						dDeque.PushBottom(value);
					// Now we process our work
					while (dDeque.PopBottom(out value) == PopResult.Succeed) {
						if (value != null) {
							value.threadStart();
						}
					}
				}
				
				// When we have finished, steal from other worker
				ThreadWorker other;
				// Repeat the operation a little so that we can let other things process.
				for (int j = 0; j < maxRetry; j++) {
					// Start stealing with the ThreadWorker at our right to minimize contention
					for (int it = stealingStart; it < stealingStart + workerLength; it++) {
						int i = it % workerLength;
						if ((other = others[i]) == null || other == this)
							continue;
						while (other.dDeque.PopTop(out value) == PopResult.Succeed) {
							if (value != null) {
								value.threadStart();
							}
						}
					}
				}
			} while (!sharedWorkQueue.IsEmpty);
			//Console.WriteLine("End participation of " + this.workerThread.ManagedThreadId);
		}
		
		// Almost same as above but with an added predicate and treating one item at a time. 
		// It's used by Scheduler Participate(...) method for special waiting case like
		// Task.WaitAll(someTasks) or Task.WaitAny(someTasks)
		internal void WorkerMethod(Func<bool> predicate)
		{	
			while (!predicate()) {
				Task value;
				
				// Dequeue only one item as we have restriction
				if (sharedWorkQueue.TryPop(out value)) {
					if (value != null) {
						value.threadStart();
					}
				}
				// First check to see if we comply to predicate
				if (predicate()) {
					return;
				}
				
				// Try to complete other work by stealing since our desired tasks may be in other worker
				ThreadWorker other;
				for (int i = 0; i < workerLength; i++) {
					if ((other = others[i]) == null || other == this)
						continue;
					if (other.dDeque.PopTop(out value) == PopResult.Succeed) {
						if (value != null) {
							value.threadStart();
						}
					}
					if (predicate()) {
						return;
					}
				}
			}
		}
		
		internal bool Finished {
			get {
				return started == 0;	
			}
		}
		
		public bool Equals(ThreadWorker other)
		{
			return (other == null) ? false : object.ReferenceEquals(this.dDeque, other.dDeque);	
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
