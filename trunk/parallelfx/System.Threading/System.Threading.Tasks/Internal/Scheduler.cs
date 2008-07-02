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
using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Threading.Tasks
{
	internal class Scheduler: IScheduler
	{
		ConcurrentStack<ThreadStart> workQueue = new ConcurrentStack<ThreadStart>();
		ThreadWorker[] workers;
		int maxWorker;
		
		public Scheduler(int maxWorker)
		{
			// We put the -1 because the thread owning the Scheduler can
			// also be used as a worker
			this.maxWorker = maxWorker;
			workers = new ThreadWorker[maxWorker];
			// -1 because the last ThreadWorker of the list is the one who call Participate
			for (int i = 0; i < maxWorker - 1; i++) {
				workers[i] = new ThreadWorker(workers, workQueue);
			}
		}
		
		public void AddWork(ThreadStart func)
		{
			//Console.WriteLine("Adding work");
			// Add to the shared work pool
			workQueue.Add(func);
			// Wake up some worker if they were asleep
			PulseAll();
		}
		
		// This should be called when the user call Task.WaitAll() causing the user's thread to become
		// a ThreadWorker too via the Scheduler
		public void Participate()
		{
			ThreadWorker participant = new ThreadWorker(workers, workQueue, false);
			workers[maxWorker - 1] = participant;
			participant.WorkerMethod();
			// No more work, end the participation
			workers[maxWorker - 1] = null;
		}
		
		public void ParticipateUntil(Task task)
		{
			if (task.IsCompleted)
				return;
			
			int finished = 0;
			task.Completed += delegate { finished = 1; };
			ThreadWorker participant = GetLocalThreadWorker();
			
			participant.WorkerMethod(delegate {
				return finished == 1;	
			});
		}
		
		// Called with Task.WaitAll(someTasks) or Task.WaitAny(someTasks) so that we can remove ourselves
		// also when our wait condition is ok
		public void ParticipateUntil(IEnumerable<Task> tasks, Func<int, bool> predicate)
		{
			int numFinished = 0;

			foreach (Task t in tasks) {
				t.Completed += delegate { Interlocked.Increment(ref numFinished); };	
			}
			
			ThreadWorker participant = GetLocalThreadWorker();
			
			// predicate for WaitAny would be numFinished == 1 and for WaitAll numFinished == count
			participant.WorkerMethod(delegate {
				return predicate(numFinished);
			});
		}
		
		// Hacky
		public void EnsureEverybodyFinished()
		{
			bool c = true;
			while (c) {
				Thread.Sleep(5);
				bool temp = true;
				for (int i = 0; i < maxWorker; i++) {
					ThreadWorker w = workers[i];
					if (w != null)
						temp &= w.Finished;
					if (!temp)
						break;
				}
				c = !temp;
			}
		}
		
		void PulseAll()
		{
			foreach (ThreadWorker worker in workers) {
				if (worker != null)
					worker.Pulse();	
			}
		}
		
		// Replace that by LazyInit<T> when gmcs can compile it
		ThreadWorker participant = null;
		ThreadWorker GetLocalThreadWorker()
		{
			// This one has no participation as it has no Dequeue suitable for stealing
			// that's why it's not in the workers array
			if (participant == null)
				participant = new ThreadWorker(workers, workQueue, false);
			// It's ok to do the lazy init like this as there is only one thread which call this method (if the user don't mess up !)
			return participant;
		}
	}
}
