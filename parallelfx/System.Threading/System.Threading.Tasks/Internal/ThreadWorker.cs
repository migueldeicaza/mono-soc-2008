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
		static Random r = new Random();
		
		Thread workerThread;
		
		readonly          IScheduler            sched;
		readonly          ThreadWorker[]        others;
		internal readonly DynamicDeque<Task>    dDeque;
		readonly          ConcurrentStack<Task> sharedWorkQueue;
		//readonly        OptimizedStack<Task>  sharedWorkQueue;
		
		// Flag to tell if workerThread is running
		int started = 0; 
		
		readonly bool isLocal;
		readonly int  workerLength;
		readonly int  stealingStart;
		const    int  maxRetry = 5;
		
		#region Sleep related fields
		const int sleepTimeBeforeRetry = 1;
		const int sleepThreshold = 100000;
		#endregion
		
		Action threadInitializer;
		
		public ThreadWorker(IScheduler sched, ThreadWorker[] others, ConcurrentStack<Task> sharedWorkQueue,
		                    int maxStackSize, ThreadPriority priority):
			this(sched, others, sharedWorkQueue, true, maxStackSize, priority)
		{
			
		}
		
		public ThreadWorker(IScheduler sched, ThreadWorker[] others, ConcurrentStack<Task> sharedWorkQueue,
		                    bool createThread, int maxStackSize, ThreadPriority priority)
		{
			this.sched           = sched;
			this.others          = others;
			this.dDeque          = new DynamicDeque<Task>();
			this.sharedWorkQueue = sharedWorkQueue;
			this.workerLength    = others.Length;
			this.isLocal         = !createThread;
			
			// Find the stealing start index randomly (then the traversal
			// will be done in Round-Robin fashion)
			do {
				this.stealingStart = r.Next(0, workerLength);
			} while (others[stealingStart] == this);
			
			InitializeUnderlyingThread(maxStackSize, priority);
		}
		
		void InitializeUnderlyingThread(int maxStackSize, ThreadPriority priority)
		{
			threadInitializer = delegate {
				// Special case of the participant ThreadWorker
				if (isLocal) {			
					this.workerThread = Thread.CurrentThread;
					return;
				}
				
				this.workerThread = (maxStackSize == 0) ? new Thread(WorkerMethodWrapper) :
						new Thread(WorkerMethodWrapper, maxStackSize);
	
				this.workerThread.IsBackground = true;
				this.workerThread.Priority = priority;
			};
			threadInitializer();
		}
		
		public void Pulse()
		{
			// If the thread was stopped then set it in use and restart it
			int result = Interlocked.Exchange(ref started, 1);
			if (result != 0)
				return;
			if (!isLocal) {
				if (this.workerThread.ThreadState != ThreadState.Unstarted) {
					threadInitializer();
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
		
		// This is the actual method called in the Thread
		void WorkerMethodWrapper()
		{
			Task.childWorkAdder = (t) => { 
				dDeque.PushBottom(t);
				sched.PulseAll();
			};
			int sleepTime = 0;
			// Main loop
			while (started == 1) {
				WorkerMethod();
				Thread.Sleep(sleepTimeBeforeRetry);
				// If the Thread has been more sleeping than working shut it down
				if (sleepTime++ > sleepThreshold)
					break;
			}
			
			started = 0;
		}
		
		// Main method, used to do all the logic of retrieving, processing and stealing work.
		void WorkerMethod()
		{
			bool hasStolenFromOther;
			do {
				hasStolenFromOther = false;
				Task value;
				// We fill up our work deque concurrently with other ThreadWorker
				while (!sharedWorkQueue.IsEmpty) {
					while (sharedWorkQueue.TryPop(out value)) {
						dDeque.PushBottom(value);
					}
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
						// Maybe make this steal more than one item at a time, see TODO.
						if (other.dDeque.PopTop(out value) == PopResult.Succeed) {
							hasStolenFromOther = true;
							if (value != null) {
								value.threadStart();
							}
						}
					}
				}
			} while (!sharedWorkQueue.IsEmpty || hasStolenFromOther);
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
				for (int it = stealingStart; it < stealingStart + workerLength; it++) {
					int i = it % workerLength;
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
		
		public bool Finished {
			get {
				return started == 0;	
			}
		}
		
		public bool IsLocal {
			get {
				return isLocal;
			}
		}
		
		public int Id {
			get {
				return workerThread.ManagedThreadId;
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
