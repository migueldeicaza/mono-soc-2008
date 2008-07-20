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
		public const int DefaultDop = -1;
		
		internal static int Dop<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			return temp == null ? DefaultDop : temp.Dop;
		}
		
		internal static void IsNotLast<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			if (temp == null)
				return;
			temp.IsLast = false;
			//Console.WriteLine("Correctly IsNotLast-ed");
		}
		
		internal static IParallelEnumerator<T> GetParallelEnumerator<T>(this IParallelEnumerable<T> source)
		{
			IParallelEnumerator<T> temp = source.GetEnumerator() as IParallelEnumerator<T>;
			return temp;
		}
		
		internal static IParallelEnumerator<T> GetSynchronousParallelEnumerator<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			return temp == null ? null : temp.GetSynchronousParallelEnumerator();
		}
		
		static void Process<TSource>(IParallelEnumerable<TSource> source, Func<int, TSource, bool> action, bool block)
		{
			source.IsNotLast();
			IParallelEnumerator<TSource> feedEnum = source.GetParallelEnumerator();
			int index = -1;
			
			Parallel.SpawnBestNumber(delegate {
				TSource item;
				while (feedEnum.MoveNext(out item)) {
					// Technically it's not exact, we might get preempted
					int i = Interlocked.Increment(ref index);
					if (!action(i, item))
						break;
				}
			}, source.Dop(), block, null);
		}
		
		static IParallelEnumerable<TResult> Process<TSource, TResult>(IParallelEnumerable<TSource> source,
		                                                    Func<Action<TResult>, int, TSource, bool> action)
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
					return action (adder, i, item);
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
				return true;
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
				return true;
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
				return true;
			}, true);
			return count;
		}
		#endregion
		
		#region Any
		public static bool Any<TSource>(this IParallelEnumerable<TSource> source)
		{
			bool result = false;
			Process(source, delegate (int i, TSource element) {
				result = true;
				return false;
			}, true);
			
			return result;
		}
		#endregion
		
		#region Aggregate
		public static TResult Aggregate<TSource, TAccumulate, TResult>(this IParallelEnumerable<TSource> source,
		                                                               Func<TAccumulate> seedFactory,
		                                                               Func<TAccumulate, TSource, TAccumulate> intermediateReduceFunc,
		                                                               Func<TAccumulate, TAccumulate, TAccumulate> finalReduceFunc,
		                                                               Func<TAccumulate, TResult> resultSelector)
		{
			int count = Parallel.GetBestWorkerNumber();
			TAccumulate[] accumulators = new TAccumulate[count];
			for (int i = 0; i < count; i++) {
				accumulators[i] = seedFactory();
			}
			
			int index = -1;
			Process<TSource>(source, delegate (int j, TSource element) {
				int i = Interlocked.Increment(ref index) % count;
				// Reduce results on each domain
				accumulators[i] = intermediateReduceFunc(accumulators[i], element);
				return true;
			}, true);
			// Reduce the final domains into a single one
			for (int i = 1; i < count; i++) {
				accumulators[0] = finalReduceFunc(accumulators[0], accumulators[i]);
			}
			// Return the final result
			return resultSelector(accumulators[0]);
		}
		#endregion
		
		#region Concat
		public static IParallelEnumerable<TSource> Concat<TSource>(this IParallelEnumerable<TSource> source,
		                                                           IParallelEnumerable<TSource> second)
		{
			source.IsNotLast();
			second.IsNotLast();
			IParallelEnumerable<TSource> temp = 
				ParallelEnumerableFactory.GetFromIParallelEnumerable(source.Dop(), source, second);
			
			return Process<TSource, TSource>(temp, delegate (Action<TSource> adder, int i, TSource e) {
				adder(e);
				return true;
			});
		}
		#endregion
		
		#region Take
		public static IParallelEnumerable<TSource> Take<TSource>(this IParallelEnumerable<TSource> source, int count)
		{
			int counter = 0;
			
			return Process<TSource, TSource> (source, delegate (Action<TSource> adder, int i, TSource e) {
				if (Interlocked.Increment(ref counter) <= count) {
					adder(e);
					return true;
				} else {
					return false;
				}
			});
		}
		#endregion
		
		#region Skip
		public static IParallelEnumerable<TSource> Skip<TSource>(this IParallelEnumerable<TSource> source, int count)
		{
			int counter = 0;
			
			return source.Where((element, index) => Interlocked.Increment(ref counter) >= count);
		}
		#endregion
		
		#region ElementAt
		public static TSource ElementAt<TSource>(this IParallelEnumerable<TSource> source, int index)
		{
			TSource result = default(TSource);
			int currIndex = -1;
			
			Process(source, delegate (int j, TSource element) {
				if (Interlocked.Increment(ref currIndex) == index) {
					result = element;
					return false;
				}
				return true;
			}, true);
			
			return result;
		}
		#endregion
		
		#region Range & Repeat
		public static IParallelEnumerable<int> Range(int start, int count)
		{
			return ParallelEnumerableFactory.GetFromRange (start, count, DefaultDop);
		}
		
		public static IParallelEnumerable<TResult> Repeat<TResult>(TResult element, int count)
		{
			int numTime = -1;
			
			Func<Action<TResult>, bool> action = delegate (Action<TResult> adder) {
				int temp = Interlocked.Increment(ref numTime);
				if (temp >= count)
				    return false;
				adder(element);
				return true;
			};
			
			return ParallelEnumerableFactory.GetFromBlockingCollection(new BlockingCollection<TResult>(new ConcurrentStack<TResult>(), count)
			                                                           , action, DefaultDop);
		}
		#endregion
	}
}
