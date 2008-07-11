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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading
{
	
	public static class Parallel
	{
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
			TaskManagerPolicy policy = TaskManager.Current.Policy;
			int part = 40 / policy.IdealProcessors;
			part = Math.Max(5, part);
			int pcount = (to - from) / part;
			if (pcount == 0)
				pcount = 1;
			
			int start = from;
			Task[] tasks = new Task[pcount];
			
			ParallelState state = new ParallelState(tasks);
			
			for (int i = 0; i < pcount; i++) {
				int pstart = start + i * part;
				int pend = (i == pcount - 1) ? to : pstart + part;
				tasks[i] = Task.Create(delegate {
					for (int j = pstart; j < pend; j += step)
						action(j, state);
				});
			}
			Task.WaitAll(tasks);
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
			throw new NotImplementedException();
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
			foreach (Action a in actions) {
				Task.Create(_ => a());
			}
		}
		
		public static void Invoke(Action[] actions, TaskManager tm, TaskCreationOptions tco)
		{
			foreach (Action a in actions) {
				Task.Create(_ => a(), tm, tco);
			}
		}
	}
}
