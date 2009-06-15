//#if NET_4_0
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
		static Parallel ()
		{
			string str = Environment.GetEnvironmentVariable ("MAX_FOR_COUNT");
			int count;
			if (!string.IsNullOrEmpty (str) && int.TryParse (str, out count))
				MaxForCount = count;
			else
				MaxForCount = 1;
		}
		
		public static int GetBestWorkerNumber ()
		{
			return GetBestWorkerNumber (TaskScheduler.Current);
		}
		
		public static int GetBestWorkerNumber (TaskScheduler scheduler)
		{	
			return scheduler.MaximumConcurrencyLevel;
		}
		
		static void HandleExceptions (IEnumerable<Task> tasks)
		{
			List<Exception> exs = new List<Exception> ();
			foreach (Task t in tasks) {
				if (t.Exception != null && !(t.Exception is TaskCanceledException))
					exs.Add (t.Exception);
			}
			
			if (exs.Count > 0) {
				throw new AggregateException (exs);
			}
		}
		
		static void InitTasks (Task[] tasks, Action<object> action, int count)
		{
			InitTasks (tasks, count, () => Task.StartNew (action, TaskCreationOptions.Detached));
		}
		
		static void InitTasks (Task[] tasks, int count, Func<Task> taskCreator)
		{
			for (int i = 0; i < count; i++) {
				tasks [i] = taskCreator ();
			}
		}
		
		static void InitCleanerCallback<TLocal> (Task[] tasks, ParallelState<TLocal> state, Action<TLocal> cleanFunc)
		{
			Action<Task> cleanCallback = delegate (Task t) {
				cleanFunc (state.ThreadLocalState);
			};
			foreach (Task t in tasks)
				t.ContinueWith (cleanCallback, TaskContinuationKind.OnAny, TaskCreationOptions.None, true);
		}
		
		#region For
		
		static readonly int MaxForCount;
		
		public static void For (int from, int to, Action<int> action)
		{
			For (from, to, 1, action);
		}
		
		public static void For (long from, long to, Action<long> action)
		{
			For (from, to, step, (i, state) => action (i));
		}
		
		public static void For (int from, int to, Action<int, ParallelLoopState> action)
		{
			For (from, to, 1, action);
		}
		
		public static void For (long from, long to, Action<long, ParallelLoopState> action)
		{
			For (from, to, 1, action);
		}
		
		public static void For (int from, int to, ParallelOptions options, Action<int> action)
		{
			For (from, to, 1, action);
		}
		
		public static void For (long from, long to, ParallelOptions options, Action<long> action)
		{
			For (from, to, step, (i, state) => action (i));
		}
		
		public static void For (int from, int to, ParallelOptions options, Action<int, ParallelLoopState> action)
		{
			For (from, to, 1, action);
		}
		
		public static void For (long from, long to, ParallelOptions options, Action<long, ParallelLoopState> action)
		{
			For (from, to, 1, action);
		}
		
		public static void For (int from, int to, int step, Action<int, ParallelState> action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			
			int num = GetBestWorkerNumber ();

			Task[] tasks = new Task [num];
			ParallelState state = new ParallelState (tasks);
			
			int currentIndex = from;
			
			Action<object> workerMethod = delegate {
				int index;
				/*while ((index = Interlocked.Add (ref currentIndex, step) - step) < to && !state.IsStopped) {
					action (index, state);
				}*/
				int count = MaxForCount * step;
				while ((index = Interlocked.Add (ref currentIndex, count) - (count)) < to && !state.IsStopped) {
					for (int i = index; i < to && i < index + (count); i += step) {
						action (i, state);
					}
				}
			};
			
			InitTasks (tasks, workerMethod, num);
			Task.WaitAll (tasks);
			HandleExceptions (tasks);
		}
		
		public static void For<TLocal> (int fromInclusive, int toExclusive, Func<TLocal> threadLocalSelector,
		                                Action<int, ParallelState<TLocal>> body)
		{
			For<TLocal>(fromInclusive, toExclusive, 1, threadLocalSelector, body);
		}
		
		public static void For<TLocal> (int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                                Action<int, ParallelState<TLocal>> body)
		{
			For<TLocal>(fromInclusive, toExclusive, step, threadLocalSelector, body, null);
		}
		
		public static void For<TLocal> (int fromInclusive, int toExclusive, Func<TLocal> threadLocalSelector,
		                                Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup)
		{
			For<TLocal>(fromInclusive, toExclusive, 1, threadLocalSelector, body, threadLocalCleanup);
		}
		
		public static void For<TLocal> (int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                                Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup)
		{
			if (body == null)
				throw new ArgumentNullException ("body");
			if (step < 0)
				throw new ArgumentOutOfRangeException ("step", "step must be positive");
			if (threadLocalSelector == null)
				throw new ArgumentNullException ("threadLocalSelector");
			
			For<TLocal> (fromInclusive, toExclusive, step, threadLocalSelector,
			             body, threadLocalCleanup, (a, count, act) => InitTasks (a, act, count));
		}
		
		public static void For<TLocal> (int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                                Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup,
		                                TaskManager manager, TaskCreationOptions options)
		{
			if (body == null)
				throw new ArgumentNullException ("body");
			if (step < 0)
				throw new ArgumentOutOfRangeException ("step", "step must be positive");
			if (threadLocalSelector == null)
				throw new ArgumentNullException ("threadLocalSelector");
			if (manager == null)
				throw new ArgumentNullException ("manager");
			
			For<TLocal>(fromInclusive, toExclusive, step, threadLocalSelector,
			            body, threadLocalCleanup,
			            (a, count, act) => InitTasks (a, count, () => Task.StartNew (act, manager, options)));
		}
		
		public static void For<TLocal> (int fromInclusive, int toExclusive, int step, Func<TLocal> threadLocalSelector,
		                                Action<int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup,
		                                Action<Task[], int, Action<object>> tasksCreator)
		{
			int num = GetBestWorkerNumber ();
			
			Task[] tasks = new Task [num];
			ParallelState<TLocal> state = new ParallelState<TLocal> (tasks, threadLocalSelector);
			
			int currentIndex = fromInclusive;
			
			Action<object> workerMethod = delegate {
				int index;
				while ((index = Interlocked.Add (ref currentIndex, step) - step) < toExclusive && !state.IsStopped) {
					body (index, state);
				}
			};
			
			tasksCreator (tasks, num, workerMethod);
			if (threadLocalCleanup != null)
				InitCleanerCallback (tasks, state, threadLocalCleanup);
			
			Task.WaitAll (tasks);
			HandleExceptions (tasks);
		}
		
		#endregion
		
		#region Foreach
		
		public static void ForEach<TSource> (IEnumerable<TSource> enumerable, Action<TSource> action)
		{
			ForEach (enumerable, (e, index, state) => action (e));
		}
		
		public static void ForEach<TSource> (IEnumerable<TSource> enumerable, Action<TSource,
		                                     ParallelState> action)
		{
			ForEach (enumerable, (e, index, state) => action (e, state));
		}
		
		public static void ForEach<TSource> (IEnumerable<TSource> enumerable, Action<TSource, int> action)
		{
			ForEach (enumerable, (e, index, state) => action (e, index));
		}
		
		public static void ForEach<TSource> (IEnumerable<TSource> enumerable,
		                                     Action<TSource, int, ParallelState> action)
		{
			// Unfortunately the enumerable manipulation isn't guaranteed to be thread-safe so we use
			// a light weight lock for the 3 or so operations to retrieve an element which should be fast for
			// most collection.
#if USE_MONITOR
			object syncRoot = new object ();
#else
			SpinLockWrapper sl = new SpinLockWrapper ();
#endif
			
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
#if USE_MONITOR
					lock (syncRoot) {
#else
					try {
						sl.Lock.Enter ();
#endif
						// From here it's thread-safe
						index      = currentIndex++;
						isFinished = !enumerator.MoveNext();
						if (isFinished)
							return;
						element = enumerator.Current;
						// End of thread-safety
#if USE_MONITOR
					}
#else
					} finally {
						sl.Lock.Exit ();
					}
#endif
					
					action (element, index, state);
				}
			};
			
			InitTasks (tasks, workerMethod, num);	
			
			Task.WaitAll (tasks);
			HandleExceptions (tasks);
		}
		
		public static void ForEach<TSource, TLocal> (IEnumerable<TSource> enumerable, Func<TLocal> threadLocalSelector,
		                                             Action<TSource, int, ParallelState<TLocal>> body)
		{
			ForEach<TSource, TLocal> (enumerable, threadLocalSelector, body, null);
		}
		
		public static void ForEach<TSource, TLocal> (IEnumerable<TSource> source, Func<TLocal> threadLocalSelector, 
		                                             Action<TSource, int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup)
		{
			ForEach<TSource, TLocal> (source, threadLocalSelector, body, threadLocalCleanup, (a, count, act) => InitTasks (a, act, count));
		}
		
		public static void ForEach<TSource, TLocal> (IEnumerable<TSource> source, Func<TLocal> threadLocalSelector, 
		                                             Action<TSource, int, ParallelState<TLocal>> body, Action<TLocal> threadLocalCleanup,
		                                             TaskManager manager, TaskCreationOptions options)
		{
			ForEach<TSource, TLocal> (source, threadLocalSelector, body, threadLocalCleanup,
			                          (a, count, act) => InitTasks (a, count, () => Task.StartNew (act, manager, options)));
		}

		public static void ForEach<TSource, TLocal> (IEnumerable<TSource> source, Func<TLocal> threadLocalSelector, 
		                                             Action<TSource, int, ParallelState<TLocal>> body,
		                                             Action<TLocal> threadLocalCleanup, Action<Task[], int, Action<object>> tasksCreator) 
		{
			// Unfortunately the enumerable manipulation isn't guaranteed to be thread-safe so we use
			// a light weight lock for the 3 or so operations to retrieve an element which should be fast for
			// most collection.
#if USE_MONITOR
			object syncRoot = new object ();
#else
			SpinLockWrapper sl = new SpinLockWrapper ();
#endif
			
			int num = GetBestWorkerNumber ();
			
			Task[] tasks = new Task [num];
			ParallelState<TLocal> state = new ParallelState<TLocal> (tasks, threadLocalSelector);
			
			IEnumerator<TSource> enumerator = source.GetEnumerator ();
			int currentIndex = 0;
			bool isFinished = false;
			
			Action<object> workerMethod = delegate {
				int index = -1;
				TSource element = default (TSource);
				
				while (!isFinished && !state.IsStopped) {
#if USE_MONITOR
					lock (syncRoot) {
#else
					try {
						sl.Lock.Enter ();
#endif
						// From here it's thread-safe
						index      = currentIndex++;
						isFinished = !enumerator.MoveNext ();
						if (isFinished)
							return;
						element = enumerator.Current;
						// End of thread-safety
#if USE_MONITOR
					}
#else
					} finally {
						sl.Lock.Exit ();
					}
#endif
					
					body (element, index, state);
				}
			};
			
			tasksCreator (tasks, num, workerMethod);
			if (threadLocalCleanup != null)
				InitCleanerCallback (tasks, state, threadLocalCleanup);
			Task.WaitAll (tasks);
			HandleExceptions (tasks);
		}
		
		#endregion
		
		#region While
		public static void While (Func<bool> predicate, Action body)
		{
			While (predicate, (state) => body ());
		}
		
		public static void While (Func<bool> predicate, Action<ParallelState> body)
		{
			int num = GetBestWorkerNumber ();
			
			Task[] tasks = new Task [num];
			ParallelState state = new ParallelState (tasks);
			
			Action<object> action = delegate {
				while (!state.IsStopped && predicate ())
				body (state);
			};
			
			InitTasks (tasks, action, num);
			Task.WaitAll (tasks);
			HandleExceptions (tasks);
		}
		
		#endregion

		#region Invoke
		public static void Invoke (params Action[] actions)
		{			
			Invoke (actions, (Action a) => Task.StartNew ((o) => a ()));
		}
		
		public static void Invoke (Action[] actions, TaskManager tm, TaskCreationOptions tco)
		{
			Invoke (actions, (Action a) => Task.StartNew ((o) => a (), tm, tco));
		}
		
		static void Invoke (Action[] actions, Func<Action, Task> taskCreator)
		{
			if (actions.Length == 0)
				throw new ArgumentException ("actions is empty");
			
			// Execute it directly
			if (actions.Length == 1)
				actions[0] ();
			
			Task[] ts = Array.ConvertAll (actions, delegate (Action a) {
				return taskCreator (a);
			});
			Task.WaitAll (ts);
			HandleExceptions (ts);
		}
		#endregion

		#region SpawnBestNumber, used by PLinq
		internal static Task[] SpawnBestNumber (Action action, Action callback)
		{
			return SpawnBestNumber (action, -1, callback);
		}
		
		internal static Task[] SpawnBestNumber (Action action, int dop, Action callback)
		{
			return SpawnBestNumber (action, dop, false, callback);
		}
		
		internal static Task[] SpawnBestNumber (Action action, int dop, bool wait, Action callback)
		{
			// Get the optimum amount of worker to create
			int num = dop == -1 ? (wait ? GetBestWorkerNumber () + 1 : GetBestWorkerNumber ()) : dop;
			
			// Initialize worker
			CountdownEvent evt = new CountdownEvent (num);
			Task[] tasks = new Task [num];
			for (int i = 0; i < num; i++) {
				tasks [i] = Task.StartNew (_ => { 
					action ();
					evt.Decrement ();
					if (callback != null && evt.IsSet)
						callback ();
				});
			}

			// If explicitely told, wait for all workers to complete 
			// and thus let main thread participate in the processing
			if (wait)
				Task.WaitAll (tasks);
			
			return tasks;
		}
		#endregion
	}
}
//#endif
