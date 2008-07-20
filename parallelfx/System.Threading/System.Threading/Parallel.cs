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
			TaskManagerPolicy policy = TaskManager.Current.Policy;
			
			return policy.IdealProcessors * policy.IdealThreadsPerProcessor;
		}
		
		static void HandleExceptions(IEnumerable<Task> tasks)
		{
			IEnumerable<Exception> exs = tasks.Where(t => t.Exception != null).Select(t => t.Exception);
			if (exs.Any()) {
				throw new AggregateException(exs);
			}
		}
		
		public static void For(int from, int to, Action<int> action)
		{
			For(from, to, 1, action);
		}
		
		public static void For(int from, int to, int step, Action<int> action)
		{
			int num = GetBestWorkerNumber();

			Task[] tasks = new Task[num];
			
			int currentIndex = from;
			
			Action<object> workerMethod = delegate {
				int index;
				while ((index = Interlocked.Add(ref currentIndex, step) - step) < to) {
					action (index);
				}
			};
			
			for (int i = 0; i < num; i++) {
				tasks[i] = Task.Create(workerMethod);
			}
			
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
			
			for (int i = 0; i < num; i++) {
				tasks[i] = Task.Create(workerMethod);
			}
			
			Task.WaitAll(tasks);
			HandleExceptions(tasks);
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body)
		{
			throw new NotImplementedException();
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup)
		{
			throw new NotImplementedException();
		}
		
		public static void For<TLocal>(int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                               Action<int, ParallelState<TLocal>> body)
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
			
			for (int i = 0; i < num; i++) {
				tasks[i] = Task.Create(workerMethod);
			}
			
			Task.WaitAll(tasks);
			HandleExceptions(tasks);
		}
		
		public static void ForEach<TSource, TLocal>(IEnumerable<TSource> source, Func<TLocal> threadLocalSelector,
		                                            Action<TSource, int, ParallelState<TLocal>> body)
		{
			throw new NotImplementedException();
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
			// Execute it directly
			if (actions.Length == 1)
				actions[0]();
			
			Task[] ts = Array.ConvertAll(actions, delegate (Action a) {
				return Task.Create((o) => a());
			});
			Task.WaitAll(ts);
			HandleExceptions(ts);
		}
		
		public static void Invoke(Action[] actions, TaskManager tm, TaskCreationOptions tco)
		{
			if (actions.Length == 0)
				throw new ArgumentException("actions is empty");
			// Execute it directly
			if (actions.Length == 1)
				actions[0]();
			
			Task[] ts = Array.ConvertAll(actions, delegate (Action a) {
				return Task.Create((o) => a(), tm, tco);
			});
			Task.WaitAll(ts);
			HandleExceptions(ts);
		}
		
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
			int num = dop == -1 ? GetBestWorkerNumber() : dop;
			Task[] tasks = new Task[num];
			for (int i = 0; i < num; i++) {
				tasks[i] = Task.Create(_ => action());
			}
			if (callback != null) {
				for (int j = 0; j < num; j++) {
					tasks[j].ContinueWith(delegate {
						Task.WaitAll(tasks);
						callback();
					});
				}
			}
			if (wait)
				Task.WaitAll(tasks);
			return tasks;
		}
	}
}
