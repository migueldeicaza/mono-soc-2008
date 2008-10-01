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
using System.Linq;
//using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Threading.Tasks
{
	internal class Scheduler: IScheduler
	{
		ConcurrentStack<Task> workQueue;
		//ConcurrentStack<Task>[] workBuffers;
		ThreadWorker[]        workers;
		ThreadWorker          participant;
		bool                  isPulsable = true;
		
		public Scheduler(int maxWorker, int maxStackSize, ThreadPriority priority)
		{
			//workQueue = new OptimizedStack<Task>(maxWorker);
			workQueue = new ConcurrentStack<Task>();
			workers = new ThreadWorker[maxWorker];
			
			// -1 because the last ThreadWorker of the list is the one who call Participate
			for (int i = 0; i < maxWorker - 1; i++) {
				workers[i] = new ThreadWorker(this, workers, workQueue, maxStackSize, priority);
			}
			
			participant = new ThreadWorker(this, workers, workQueue, false, 0, ThreadPriority.Normal);
		}
		
		public void AddWork(Task t)
		{
			// Add to the shared work pool
			workQueue.Push(t);
			// Wake up some worker if they were asleep
			PulseAll();
		}
		
		// Behave like a normal worker without regards for certain task completion.
		// Useful for certain application like Actors
		public void Participate()
		{
			ThreadWorker worker = GetLocalThreadWorker();
			workers[workers.Length - 1] = worker;
			worker.WorkerMethod();
			workers[workers.Length - 1] = null;
		}
		
		public void ParticipateUntil(Task task)
		{
			if (AreTasksFinished(task))
				return;
			
			ThreadWorker participant = GetLocalThreadWorker();
			
			participant.WorkerMethod(delegate {
				return AreTasksFinished(task);
			});
		}
		
		public bool ParticipateUntil(Task task, Func<bool> predicate)
		{
			if (AreTasksFinished(task))
				return false;
			
			bool isFromPredicate = false;
			ThreadWorker participant = GetLocalThreadWorker();
			
			participant.WorkerMethod(delegate {
				if (predicate()) {
					isFromPredicate = true;
					return true;
				}
				return AreTasksFinished(task);	
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
		
		public void PulseAll()
		{
			if (isPulsable) {
				foreach (ThreadWorker worker in workers) {
					if (worker != null)
						worker.Pulse();	
				}
			}
		}
		
		public void InhibitPulse()
		{
			isPulsable = false;
		}
		
		public void UnInhibitPulse() 
		{
			isPulsable = true;
		}

		public void Dispose()
		{
			foreach (ThreadWorker w in workers) {
				w.Dispose();
			}
		}
		
		// This one has no participation as it has no Dequeue suitable for stealing
		// that's why it's not in the workers array
		ThreadWorker GetLocalThreadWorker()
		{
			// It's ok to do the lazy init like this as there is only one thread which call this method (if the user don't mess up !)
			return participant;
		}
		
		bool AreTasksFinished(Task parent)
		{
			if (!parent.IsCompleted)
				return false;
			if (parent.ChildTasks.Count == 0)
				return true;
			
			return !parent.ChildTasks.Where((t) => !t.IsCompleted).Any();
		}
	}
}
