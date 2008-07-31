// Parallel.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading
{
	public static class Parallel
	{
		public static int GetBestWorkerNumber()
		{
			return GetBestWorkerNumber(TaskManager.Current.Policy);
		}
		
		public static int GetBestWorkerNumber(TaskManagerPolicy policy)
		{	
			return policy.IdealProcessors * policy.IdealThreadsPerProcessor;
		}
		
		static void HandleExceptions(IEnumerable<Task> tasks)
		{
			IEnumerable<Exception> exs = tasks.Where(t => t.Exception != null).Select(t => t.Exception);
			if (exs.Any()) {
				throw new AggregateException(exs);
			}
		}
		
		static void InitTasks(Task[] tasks, Action<object> action, int count)
		{
			for (int i = 0; i < count; i++) {
				tasks[i] = Task.Create(action);
			}
		}
		
		public static void For(int from, int to, Action<int> action)
		{
			For(from, to, 1, action);
		}
		
		public static void For(int from, int to, int step, Action<int> action)
		{
			if (action == null)
				throw new ArgumentNullException("action");
			if (step < 0)
				throw new ArgumentOutOfRangeException("step", "step must be positive");
			
			int num = GetBestWorkerNumber();

			Task[] tasks = new Task[num];
			
			int currentIndex = from;
			
			Action<object> workerMethod = delegate {
				int index;
				while ((index = Interlocked.Add(ref currentIndex, step) - step) < to) {
					action (index);
				}
			};
			
			InitTasks(tasks, workerMethod, num);
			
			Task.WaitAll(tasks);
			HandleExceptions(tasks);
		}
		
		public static void For(int from, int to, Action<int, ParallelState> action)
		{
			For(from, to, 1, action);
		}
		
		public static void For(int from, int to, int step, Action<int, ParallelState> action)
		{
			int num = GetBestWorkerNumber();

			Task[] tasks = new Task[num];
			ParallelState state = new ParallelState(tasks);
			
			int currentIndex = from;
			
			Action<object> workerMethod = delegate {
				int index;
				while ((index = Interlocked.Add(ref currentIndex, step) - step) < to && !state.IsStopped) {
					action (index, state);
				}
			};
			
			InitTasks(tasks, workerMethod, num);
			
			Task.WaitAll(tasks);
			HandleExceptions(tasks);
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body)
		{
			For<TLocal>(fromInclusive, toExclusive, 1, threadLocalSelector, body);
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body)
		{
			// TODO: could refactor this
			if (body == null)
				throw new ArgumentNullException("body");
			if (step < 0)
				throw new ArgumentOutOfRangeException("step", "step must be positive");
			if (threadLocalSelector == null)
				throw new ArgumentNullException("threadLocalSelector");
			
			int num = GetBestWorkerNumber();

			Task[] tasks = new Task[num];
			ParallelState<TLocal> state = new ParallelState<TLocal>(tasks, threadLocalSelector);
			
			int currentIndex = fromInclusive;
			
			Action<object> workerMethod = delegate {
				int index;
				while ((index = Interlocked.Add(ref currentIndex, step) - step) < toExclusive && !state.IsStopped) {
					body (index, state);
				}
			};
			
			InitTasks(tasks, workerMethod, num);
			
			Task.WaitAll(tasks);
			HandleExceptions(tasks);
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup)
		{
			throw new NotImplementedException();
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup)
		{
			throw new NotImplementedException();
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup,
		                               TaskManager manager, TaskCreationOptions options)
		{
			throw new NotImplementedException();
		}
		
		public static void ForEach<TSource>(IEnumerable<TSource> enumerable, Action<TSource> action)
		{
			ForEach(enumerable, (e, index, state) => action(e));
		}
		
		public static void ForEach<TSource>(IEnumerable<TSource> enumerable, Action<TSource, ParallelState> action)
		{
			ForEach(enumerable, (e, index, state) => action(e, state));
		}
		
		public static void ForEach<TSource>(IEnumerable<TSource> enumerable, Action<TSource, int> action)
		{
			ForEach(enumerable, (e, index, state) => action(e, index));
		}
		
		public static void ForEach<TSource>(IEnumerable<TSource> enumerable, Action<TSource, int, ParallelState> action)
		{
			// Unfortunately the enumerable manipulation isn't guaranteed to be thread-safe so we use
			// a light weight lock for the 3 or so operations to retrieve an element which should be fast for
			// most collection.
			SpinLock sl = new SpinLock(false);
			
			int num = GetBestWorkerNumber();

			Task[] tasks = new Task[num];
			ParallelState state = new ParallelState(tasks);
			
			IEnumerator<TSource> enumerator = enumerable.GetEnumerator();
			int currentIndex = 0;
			bool isFinished = false;
			
			Action<object> workerMethod = delegate {
				int index = -1;
				TSource element = default(TSource);
				
				while (!isFinished && !state.IsStopped) {
					try {
						sl.Enter();
						// From here it's thread-safe
						index      = currentIndex++;
						isFinished = !enumerator.MoveNext();
						if (isFinished) return;
						element = enumerator.Current;
						// End of thread-safety
					} finally {
						sl.Exit();
					}
					
					action(element, index, state);
				}
			};
			
			InitTasks(tasks, workerMethod, num);	
			
			Task.WaitAll(tasks);
			HandleExceptions(tasks);
		}
		
		public static void ForEach<TSource, TLocal>(IEnumerable<TSource> enumerable, Func<TLocal> threadLocalSelector,
		                                            Action<TSource, int, ParallelState<TLocal>> action)
		{
			// Unfortunately the enumerable manipulation isn't guaranteed to be thread-safe so we use
			// a light weight lock for the 3 or so operations to retrieve an element which should be fast for
			// most collection.
			SpinLock sl = new SpinLock(false);
			
			int num = GetBestWorkerNumber();

			Task[] tasks = new Task[num];
			ParallelState<TLocal> state = new ParallelState<TLocal>(tasks, threadLocalSelector);
			
			IEnumerator<TSource> enumerator = enumerable.GetEnumerator();
			int currentIndex = 0;
			bool isFinished = false;
			
			Action<object> workerMethod = delegate {
				int index = -1;
				TSource element = default(TSource);
				
				while (!isFinished && !state.IsStopped) {
					try {
						sl.Enter();
						// From here it's thread-safe
						index      = currentIndex++;
						isFinished = !enumerator.MoveNext();
						if (isFinished) return;
						element = enumerator.Current;
						// End of thread-safety
					} finally {
						sl.Exit();
					}
					
					action(element, index, state);
				}
			};
			
			InitTasks(tasks, workerMethod, num);	
			
			Task.WaitAll(tasks);
			HandleExceptions(tasks);
		}
		
		public static void ForEach<TSource, TLocal>(IEnumerable<TSource> source, Func<TLocal> threadLocalSelector, 
		                                            Action<TSource, int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup)
		{
			throw new NotImplementedException();
		}
		
		public static void ForEach<TSource, TLocal>(IEnumerable<TSource> source, Func<TLocal> threadLocalSelector, 
		                                            Action<TSource, int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup,
		                                            TaskManager manager, TaskCreationOptions options)
		{
			throw new NotImplementedException();
		}
		
		public static void Invoke(params Action[] actions)
		{
			if (actions.Length == 0)
				throw new ArgumentException("actions is empty");
			
			Invoke(actions, (Action a) => Task.Create((o) => a()));
		}
		
		public static void Invoke(Action[] actions, TaskManager tm, TaskCreationOptions tco)
		{
			if (actions.Length == 0)
				throw new ArgumentException("actions is empty");
			
			Invoke(actions, (Action a) => Task.Create((o) => a(), tm, tco));
		}
		
		public static void Invoke(Action[] actions, Func<Action, Task> taskCreator)
		{
			// Execute it directly
			if (actions.Length == 1)
				actions[0]();
			
			Task[] ts = Array.ConvertAll(actions, delegate (Action a) {
				return taskCreator(a);
			});
			Task.WaitAll(ts);
			HandleExceptions(ts);
		}
		
		
		// Used by PLinq
		internal static Task[] SpawnBestNumber(Action action, Action callback)
		{
			return SpawnBestNumber(action, -1, callback);
		}
		
		internal static Task[] SpawnBestNumber(Action action, int dop, Action callback)
		{
			return SpawnBestNumber(action, dop, false, callback);
		}
		
		internal static Task[] SpawnBestNumber(Action action, int dop, bool wait, Action callback)
		{
			// Get the optimum amount of worker to create
			int num = dop == -1 ? (wait ? GetBestWorkerNumber() : GetBestWorkerNumber() - 1) : dop;
			
			// Initialize worker
			Task[] tasks = new Task[num];
			for (int i = 0; i < num; i++) {
				tasks[i] = Task.Create(_ => action());
			}
			
			// Register wait callback if any
			if (callback != null) {
				for (int j = 0; j < num; j++) {
					tasks[j].ContinueWith(delegate {
						for (int i = 0; i < num; i++) {
							if (i == j) continue;
							tasks[i].Wait();
						}
						callback();
					});
				}
			}

			// If explicitely told, wait for all workers to complete and thus let main thread participate in the processing
			if (wait)
				Task.WaitAll(tasks);
			
			return tasks;
		}
		
		// Faire une fonctio nfactory qui renvoit une fonction paramétré correctement avec le seed et ensuite
		// passé normalement à SpawnBestNumber
		/*internal static void SpawnBestNumber<T>(Action<T>, T seed, Func<T, T> incr, int dop)
		{
			// Get the optimum amount of worker to create
			int num = dop == -1 ? GetBestWorkerNumber() : dop;
			
			// Initialize worker
			Task[] tasks = new Task[num];
			T acc = seed;
			for (int i = 0; i < num; i++) {
				T priv = acc;
				tasks[i] = Task.Create(_ => action(priv));
				acc = incr(acc);
			}
			
			Task.WaitAll(tasks);
		}*/
	}
}
