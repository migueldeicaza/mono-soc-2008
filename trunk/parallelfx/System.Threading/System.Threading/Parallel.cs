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
		static int GetBestSlice()
		{
			/*TaskManagerPolicy policy = TaskManager.Current.Policy;
			int part = 20 / policy.IdealProcessors;
			part = Math.Max(5, part);
			
			return part;*/
			return 5;
		}
		
		static void HandleExceptions(IEnumerable<Task> tasks)
		{
			IEnumerable<Exception> exs = tasks.Where(t => t.Exception != null).Select(t => t.Exception);
			if (!exs.SequenceEqual(Enumerable.Empty<Exception>())) {
				throw new AggregateException(exs);
			}
		}
		
		public static void For(int from, int to, Action<int> action)
		{
			For(from, to, 1, action);
		}
		
		public static void For(int from, int to, int step, Action<int> action)
		{
			For(from, to, step, (int i, ParallelState s) => action(i));
		}
		
		public static void For(int from, int to, Action<int, ParallelState> action)
		{
			For(from, to, 1, action);
		}
		
		public static void For(int from, int to, int step, Action<int, ParallelState> action)
		{
			int part = GetBestSlice();
			int pcount = (to - from) / part;
			if (pcount == 0) {
				pcount = 1;
				part = to - from;
			}
			
			int start = from;
			Task[] tasks = new Task[pcount];
			
			ParallelState state = new ParallelState(tasks);
			
			for (int i = 0; i < pcount && !state.IsStopped; i++) {
				int pstart = start + i * part;
				int pend = (i == pcount - 1) ? to : pstart + part;
				tasks[i] = Task.Create(delegate {
					for (int j = pstart; j < pend && !state.IsStopped; j += step)
						action(j, state);
				});
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
			int sliceCount = GetBestSlice();
			int start = 0;
			IEnumerable<TSource> slice;
			List<Task> tasks = new List<Task>();
			
			while (!(slice = enumerable.Skip(start).Take(sliceCount)).SequenceEqual(Enumerable.Empty<TSource>())) {
				IEnumerable<TSource> sliceTemp = slice;
				start += sliceCount;
				
				tasks.Add(Task.Create(delegate {
					foreach (TSource elem in sliceTemp) {
						action(elem);
					}
				}));
			}
			Task.WaitAll(tasks.ToArray());
			HandleExceptions(tasks);
		}
		
		public static void ForEach<TSource>(IEnumerable<TSource> enumerable, Action<TSource, ParallelState> action)
		{
			throw new NotImplementedException();
		}
		
		public static void ForEach<TSource>(IEnumerable<TSource> enumerable, Action<TSource, int> action)
		{
			throw new NotImplementedException();
		}
		
		public static void ForEach<TSource>(IEnumerable<TSource> enumerable, Action<TSource, int, ParallelState> action)
		{
			throw new NotImplementedException();
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
			Task[] ts = Array.ConvertAll(actions, delegate (Action a) {
				return Task.Create((o) => a());
			});
			Task.WaitAll(ts);
			HandleExceptions(ts);
		}
		
		public static void Invoke(Action[] actions, TaskManager tm, TaskCreationOptions tco)
		{
			Task[] ts = Array.ConvertAll(actions, delegate (Action a) {
				return Task.Create((o) => a(), tm, tco);
			});
			Task.WaitAll(ts);
			HandleExceptions(ts);
		}
	}
}
