// ParallelEnumerable.cs
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

namespace System.Linq
{
	public static class ParallelEnumerable
	{
		const int defaultDop = -1;
		
		static int Dop<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			return temp == null ? defaultDop : temp.Dop;
		}
		
		static void IsNotLast<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			if (temp == null)
				return;
			temp.IsLast = false;
			//Console.WriteLine("Correctly IsNotLast-ed");
		}
		
		static IParallelEnumerator<T> GetParallelEnumerator<T>(this IParallelEnumerable<T> source)
		{
			IParallelEnumerator<T> temp = source.GetEnumerator() as IParallelEnumerator<T>;
			return temp;
		}
		
		static void Process<TSource>(IParallelEnumerable<TSource> source, Action<int, TSource> action, bool block)
		{
			source.IsNotLast();
			IParallelEnumerator<TSource> feedEnum = source.GetParallelEnumerator();
			int index = -1;
			
			Parallel.SpawnBestNumber(delegate {
				TSource item;
				while (feedEnum.MoveNext(out item)) {
					// Technically it's not exact, we might get preempted
					int i = Interlocked.Increment(ref index);
					action(i, item);
				}
			}, source.Dop(), block, null);
		}
		
		static IParallelEnumerable<TResult> Process<TSource, TResult>(IParallelEnumerable<TSource> source,
		                                                    Action<Action<TResult>, int, TSource> action)
		{
			source.IsNotLast();
			
			BlockingCollection<TResult>  resultBuffer = new BlockingCollection<TResult>();
			IParallelEnumerator<TSource> feedEnum     = source.GetParallelEnumerator();
			int                          index        = -1;
			
			Func<Action<TResult>, bool> a = delegate(Action<TResult> adder) {
				TSource item;
				if (feedEnum.MoveNext (out item)) {
					//Console.WriteLine("Was able to move next, " + Thread.CurrentThread.ManagedThreadId);
					// Technically it's not exact, we might get preempted
					int i = Interlocked.Increment (ref index);
					action (adder, i, item);
					return true;
				} else {
					//Console.WriteLine("Wasn't able to move next, " + Thread.CurrentThread.ManagedThreadId);
					return false;
				}
			};
			
			return ParallelEnumerableFactory.GetFromBlockingCollection<TResult>(resultBuffer, a, source.Dop());
		}
		
		#region Select
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, TResult> selector)
		{
			return Select (source, (TSource e, int index) => selector(e));
		}
		
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, int, TResult> selector)
		{
			return Process<TSource, TResult> (source, delegate (Action<TResult> adder, int i, TSource e) {
				adder(selector(e, i));
			});
		}
		#endregion
		
		#region Where
		public static IParallelEnumerable<TSource> Where<TSource>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, bool> predicate)
		{
			return Where(source, (TSource e, int index) => predicate(e));
		}
		
		public static IParallelEnumerable<TSource> Where<TSource>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, int, bool> predicate)
		{
			return Process<TSource, TSource> (source, delegate (Action<TSource> adder, int i, TSource e) {
				if (predicate(e, i))
					adder(e);
			});
		}
		#endregion
		
		#region Count
		public static int Count<TSource>(this IParallelEnumerable<TSource> source)
		{
			return Count(source, _ => true);
		}
		
		public static int Count<TSource>(this IParallelEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			int count = 0;
			Process(source, delegate (int i, TSource element) {
				if (predicate(element))
					Interlocked.Increment(ref count);
			}, true);
			return count;
		}
		#endregion
		
		#region Range & Repeat
		public static IParallelEnumerable<int> Range(int start, int count)
		{
			return ParallelEnumerableFactory.GetFromRange (start, count, defaultDop);
		}
		
		/*public static IParallelEnumerable<TResult> Repeat<TResult>(TResult element, int count)
		{
			ConcurrentStack<TResult> stack = new ConcurrentStack<TResult>();
			// Is Parallel.For really necessary ?
			Parallel.For(0, count, delegate (int i) {
				stack.Push(element);
			});
			
			BlockingCollection<TResult> coll = new BlockingCollection<TResult>(stack, count);
			coll.CompleteAdding();
			
			return new ParallelEnumerable<TResult>(coll, defaultDop);
		}*/
		#endregion
	}
}
