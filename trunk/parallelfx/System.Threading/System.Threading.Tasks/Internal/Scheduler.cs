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
using System.Collections.ObjectModel;
using System.Threading.Collections;

namespace System.Threading.Tasks
{
	
	
	internal class Scheduler
	{
		ConcurrentQueue<ThreadStart> workQueue = new ConcurrentQueue<ThreadStart>();
		ReadOnlyCollection<ThreadWorker> workers;
		ThreadWorker[] modifiableWorkers;
		int maxWorker;
		
		public Scheduler()
		{
			// We put the -1 because the thread owning the Scheduler can
			// also be used as a WorkerThread
			maxWorker = Environment.ProcessorCount;
			modifiableWorkers = new ThreadWorker[maxWorker];
			workers = Array.AsReadOnly(modifiableWorkers);
			// -1 because the last ThreadWorker of the list is the one who call Participate
			for (int i = 0; i < maxWorker - 1; i++) {
				modifiableWorkers[i] = new ThreadWorker(workers, workQueue);
			}
		}
		
		public void AddWork(ThreadStart func)
		{
			// Add to the shared work pool
			workQueue.Add(func);
			// Wake up some worker if they were asleep
			PulseAll();
		}
		
		// This should be called when the user call Tasks.Wait() or WaitAll() causing the user's thread to become
		// a ThreadWorker too via the Scheduler
		public void Participate()
		{
			ThreadWorker participant = new ThreadWorker(workers, workQueue, false);
			modifiableWorkers[maxWorker - 1] = participant;
			participant.WorkerMethod();
			// No more work, end the participation
			modifiableWorkers[maxWorker - 1] = null;
		}
		
		void PulseAll()
		{
			foreach (var worker in workers) {
				worker.Pulse();	
			}
		}
	}
}
