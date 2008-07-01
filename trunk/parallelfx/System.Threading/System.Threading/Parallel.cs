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
			int pcount = Environment.ProcessorCount;
			int part = (to - from) / pcount;
			int start = from;
			for (int i = 0; i < pcount - 1; i++) {
				int pstart = start + i * part;
				int pend = pstart + part;
				Task.Create(delegate {
					for (int j = pstart; j < pend; j++)
						action(j);
				});
			}
			Task.Create(delegate {
				int pstart = start + (pcount - 1) * part;
				int pend = to;
				for (int j = pstart; j < pend; j++)
					action(j);
			});
		}
		
		public static void For(int from, int to, int step, Action<int> action)
		{
			throw new NotImplementedException();
		}
		
		public static void For(int from, int to, Action<int, ParallelState> action)
		{
			throw new NotImplementedException();
		}
		
		public static void For(int from, int to, int step, Action<int, ParallelState> action)
		{
			throw new NotImplementedException();
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
		
		public static void Do(params Action[] actions)
		{
			throw new NotImplementedException();
		}
		
		public static void Do(Action[] actions, TaskManager tm, TaskCreationOptions tco)
		{
			throw new NotImplementedException();
		}
	}
}
