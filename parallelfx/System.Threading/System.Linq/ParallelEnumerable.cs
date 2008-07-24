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
		
		#region Internally used operators
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
		
		internal static void IsLast<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			if (temp == null)
				return;
			temp.IsLast = true;
			//Console.WriteLine("Correctly IsNotLast-ed");
		}
		
		internal static bool IsOrdered<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			if (temp == null)
				return false;
			return temp.IsOrdered;
		}
		
		internal static void SetUnordered<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			if (temp == null)
				return;
			temp.IsOrdered = false;
		}
		
		internal static void SetOrdered<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			if (temp == null)
				return;
			temp.IsOrdered = true;
		}
		
		internal static IParallelEnumerator<T> GetParallelEnumerator<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			return temp.GetParallelEnumerator();
		}
		#endregion
		
		static void Process<TSource>(IParallelEnumerable<TSource> source, Func<int, TSource, bool> action, bool block)
		{
			source.IsNotLast();
			IParallelEnumerator<TSource> feedEnum = source.GetParallelEnumerator();
			
			Parallel.SpawnBestNumber(delegate {
				TSource item;
				int i;
				while (feedEnum.MoveNext(out item, out i)) {
					if (!action(i, item))
						break;
				}
			}, source.Dop(), block, null);
		}
		
		static void Process<TSource>(IParallelEnumerable<TSource> source, Action<int, TSource> action, bool block)
		{
			source.IsNotLast();
			IParallelEnumerator<TSource> feedEnum = source.GetParallelEnumerator();
			
			Parallel.SpawnBestNumber(delegate {
				TSource item;
				int i;
				while (feedEnum.MoveNext(out item, out i)) {
					action(i, item);
				}
			}, source.Dop(), block, null);
		}
		
		static IParallelEnumerable<TResult> Process<TSource, TResult>(IParallelEnumerable<TSource> source,
		                                                    Func<Action<TResult, bool, int>, int, TSource, bool> action)
		{
			source.IsNotLast();
			
			Func<IParallelEnumerator<TSource>, Action<TResult, bool, int>, bool> a 
			                  = delegate(IParallelEnumerator<TSource> feedEnum, Action<TResult, bool, int> adder) {
				TSource item;
				int i;
				if (feedEnum.MoveNext (out item, out i)) {
					return action (adder, i, item);
				} else {
					return false;
				}
			};
			
			ParallelEnumerableBase<TResult> result = ParallelEnumerableFactory.GetFromBlockingCollection<TSource, TResult>(a, source);
			if (source.IsOrdered())
				result.IsOrdered = true;
			
			return result;
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
			return Process<TSource, TResult> (source, delegate (Action<TResult, bool, int> adder, int i, TSource e) {
				adder(selector(e, i), true, i);
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
			return Process<TSource, TSource> (source, delegate (Action<TSource, bool, int> adder, int i, TSource e) {
				if (predicate(e, i))
					// TODO: Make the given back index correct 
					adder(e, true, i);
				else
					adder(default(TSource), false, i);
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
			source.IsNotLast();
			bool result = source.GetParallelEnumerator().MoveNext();
			source.IsLast();
			
			return result;
		}
		#endregion
		
		#region ForAll
		public static void ForAll<T>(this IParallelEnumerable<T> source, Action<T> action)
		{
			Process(source, (i, e) => { action(e); }, true);
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
			
			return Process<TSource, TSource>(temp, delegate (Action<TSource, bool, int> adder, int i, TSource e) {
				adder(e, true, i);
				return true;
			});
		}
		#endregion
		
		#region Take
		public static IParallelEnumerable<TSource> Take<TSource>(this IParallelEnumerable<TSource> source, int count)
		{
			int counter = 0;
			
			return Process<TSource, TSource> (source, delegate (Action<TSource, bool, int> adder, int i, TSource e) {
				if (Interlocked.Increment(ref counter) <= count) {
					adder(e, true, i);
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
			
			return source.Where((element, index) => Interlocked.Increment(ref counter) > count);
		}
		#endregion
		
		#region ElementAt
		public static TSource ElementAt<TSource>(this IParallelEnumerable<TSource> source, int index)
		{
			TSource result = default(TSource);
			int currIndex = -1;
			
			Process(source, delegate (int j, TSource element) {
				int myIndex = Interlocked.Increment(ref currIndex);
				if (myIndex == index) {
					result = element;
					return false;
				}
				return true;
			}, true);
			
			return result;
		}
		#endregion
		
		#region First
		public static TSource First<TSource>(this IParallelEnumerable<TSource> source)
		{
			TSource first;
			int index;
			
			source.IsNotLast();
			bool result = source.GetParallelEnumerator().MoveNext(out first, out index);
			source.IsLast();
			
			if (!result)
				throw new InvalidOperationException("source is empty");
			
			return first;
		}
		
		public static TSource FirstOrDefault<TSource>(this IParallelEnumerable<TSource> source)
		{
			TSource first;
			int index;
			
			source.IsNotLast();
			bool result = source.GetParallelEnumerator().MoveNext(out first, out index);
			source.IsLast();
			
			return result ? first : default(TSource);
		}
		#endregion
		
		#region Zip
		public static IParallelEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IParallelEnumerable<TFirst> first,
		                                                                         IEnumerable<TSecond> second,
		                                                                         Func<TFirst, TSecond, TResult> resultSelector)
		{
			return null;
		}
		#endregion
		
		#region Range & Repeat
		public static IParallelEnumerable<int> Range(int start, int count)
		{
			return ParallelEnumerableFactory.GetFromRange (start, count, DefaultDop);
		}
		
		public static IParallelEnumerable<TResult> Repeat<TResult>(TResult element, int count)
		{
			return ParallelEnumerableFactory.GetFromRepeat<TResult>(element, count, DefaultDop);
		}
		#endregion
	}
}
