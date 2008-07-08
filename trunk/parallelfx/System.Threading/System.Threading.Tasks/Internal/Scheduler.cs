// Scheduler.cs
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
using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Threading.Tasks
{
	internal class Scheduler: IScheduler
	{
		OptimizedStack<ThreadStart> workQueue;
		ThreadWorker[] workers;
		int maxWorker;
		
		public Scheduler(int maxWorker, int maxStackSize, ThreadPriority priority)
		{
			this.maxWorker = maxWorker;
			workQueue = new OptimizedStack<ThreadStart>(maxWorker);
			workers = new ThreadWorker[maxWorker];
			
			participant = new LazyInit<ThreadWorker>(delegate {
				return new ThreadWorker(workers, workQueue, false, 0, ThreadPriority.Normal);
			});
			
			// -1 because the last ThreadWorker of the list is the one who call Participate
			for (int i = 0; i < maxWorker - 1; i++) {
				workers[i] = new ThreadWorker(workers, workQueue, maxStackSize, priority);
			}
		}
		
		public void AddWork(ThreadStart func)
		{
			// Add to the shared work pool
			workQueue.Push(func);
			// Wake up some worker if they were asleep
			PulseAll();
		}
		
		public void ParticipateUntil(Task task)
		{
			if (task.IsCompleted)
				return;
			
			ThreadWorker participant = GetLocalThreadWorker();
			
			participant.WorkerMethod(delegate {
				return task.IsCompleted;	
			});
		}
		
		public bool ParticipateUntil(Task task, Func<bool> predicate)
		{
			if (task.IsCompleted)
				return false;
			
			bool isFromPredicate = false;
			ThreadWorker participant = GetLocalThreadWorker();
			
			participant.WorkerMethod(delegate {
				if (predicate()) {
					isFromPredicate = true;
					return true;
				}
				return task.IsCompleted;	
			});
				
			return isFromPredicate;
		}
		
		// Called with Task.WaitAll(someTasks) or Task.WaitAny(someTasks) so that we can remove ourselves
		// also when our wait condition is ok
		public void ParticipateUntil(Func<bool> predicate)
		{
			ThreadWorker participant = GetLocalThreadWorker();
			
			participant.WorkerMethod(predicate);
		}
		
		
		
		void PulseAll()
		{
			foreach (ThreadWorker worker in workers) {
				if (worker != null)
					worker.Pulse();	
			}
		}

		LazyInit<ThreadWorker> participant;
		
		// This one has no participation as it has no Dequeue suitable for stealing
		// that's why it's not in the workers array
		ThreadWorker GetLocalThreadWorker()
		{
			// It's ok to do the lazy init like this as there is only one thread which call this method (if the user don't mess up !)
			return participant.Value;
		}
	}
}
