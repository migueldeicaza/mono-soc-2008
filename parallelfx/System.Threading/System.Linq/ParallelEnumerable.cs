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
		
		internal static IList<T> AsIList<T>(this IParallelEnumerable<T> source)
		{
			return AsInterface<T, IList<T>>(source);
		}
		
		internal static ICollection<T> AsICollection<T>(this IParallelEnumerable<T> source)
		{
			return AsInterface<T, ICollection<T>>(source);
		}
		
		static U AsInterface<T, U>(IParallelEnumerable<T> source) where U : class
		{
			// For PERange and PERepeat
			var coll = source as U;
			if (coll != null)
				return coll;
			
			var peEnumerable = source as PEIEnumerable<T>;
			if (peEnumerable != null)
				coll = peEnumerable.Enumerable as U;
			
			return coll;
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
			
			var coll = source.AsICollection();
			if (coll != null) return coll.Count;
			
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
			// Little short-circuit
			var coll = source.AsICollection();
			if (coll != null) return coll.Count > 0;
			
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
		
		#region OrderBy
		public static IParallelEnumerable<TSource> OrderBy<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                  Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
		{
			return OrderByInternal<TSource>(source, (e1, e2) => keySelector(e1).CompareTo(keySelector(e2)));
		}
		
		public static IParallelEnumerable<TSource> OrderBy<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                  Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			return OrderByInternal<TSource>(source, (e1, e2) => comparer.Compare(keySelector(e1), keySelector(e2)));
		}
		
		public static IParallelEnumerable<TSource> OrderByDescending<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                  Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
		{
			return OrderByInternal<TSource>(source, (e1, e2) => keySelector(e2).CompareTo(keySelector(e1)));
		}
		
		public static IParallelEnumerable<TSource> OrderByDescending<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                  Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			return OrderByInternal<TSource>(source, (e1, e2) => comparer.Compare(keySelector(e2), keySelector(e1)));
		}
		
		static IParallelEnumerable<TSource> OrderByInternal<TSource>(IParallelEnumerable<TSource> source,
		                                                           System.Comparison<TSource> comparer)
		{
			List<TSource> temp = source.Aggregate(() => new List<TSource>(),
			                                      (list, e) => { list.Add(e); return list; },
			                                      (list, list2) => { list.AddRange(list2); return list; },
			                                      (list) => list);
			
			temp.Sort(comparer);
			
			IParallelEnumerable<TSource> enumerable = ParallelEnumerableFactory.GetFromIEnumerable(temp, source.Dop());
			// OrderBy explicitely turn on ordering
			enumerable.SetOrdered();
			
			return enumerable;
		}
		#endregion
		
		#region Min - Max
		public static int Min(this IParallelEnumerable<int> source)
		{
			return BestOrder(source, (first, second) => first < second, int.MaxValue);
		}
		
		public static byte Min(this IParallelEnumerable<byte> source)
		{
			return BestOrder(source, (first, second) => first < second, byte.MaxValue);
		}
		
		public static short Min(this IParallelEnumerable<short> source)
		{
			return BestOrder(source, (first, second) => first < second, short.MaxValue);
		}
		
		public static long Min(this IParallelEnumerable<long> source)
		{
			return BestOrder(source, (first, second) => first < second, long.MaxValue);
		}
		
		public static float Min(this IParallelEnumerable<float> source)
		{
			return BestOrder(source, (first, second) => first < second, float.MaxValue);
		}
		
		public static double Min(this IParallelEnumerable<double> source)
		{
			return BestOrder(source, (first, second) => first < second, double.MaxValue);
		}
		
		public static decimal Min(this IParallelEnumerable<decimal> source)
		{
			return BestOrder(source, (first, second) => first < second, decimal.MaxValue);
		}
		
		public static int Max(this IParallelEnumerable<int> source)
		{
			return BestOrder(source, (first, second) => first > second, int.MinValue);
		}
		
		public static byte Max(this IParallelEnumerable<byte> source)
		{
			return BestOrder(source, (first, second) => first > second, byte.MinValue);
		}
		
		public static short Max(this IParallelEnumerable<short> source)
		{
			return BestOrder(source, (first, second) => first > second, short.MinValue);
		}
		
		public static long Max(this IParallelEnumerable<long> source)
		{
			return BestOrder(source, (first, second) => first > second, long.MinValue);
		}
		
		public static float Max(this IParallelEnumerable<float> source)
		{
			return BestOrder(source, (first, second) => first > second, float.MinValue);
		}
		
		public static double Max(this IParallelEnumerable<double> source)
		{
			return BestOrder(source, (first, second) => first > second, double.MinValue);
		}
		
		public static decimal Max(this IParallelEnumerable<decimal> source)
		{
			return BestOrder(source, (first, second) => first > second, decimal.MinValue);
		}
		
		// For bestSelector, if first arg is better than second returns true
		static T BestOrder<T>(IParallelEnumerable<T> source, Func<T, T, bool> bestSelector, T seed)
		{
			T best = seed;
			
			best = source.Aggregate(() => seed,
			                        (first, second) => (bestSelector(first, second)) ? first : second,
			                        (first, second) => (bestSelector(first, second)) ? first : second,
			                        (e) => e);
			return best;
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
			// Little short-circuit taken from Enumerable.cs
			var list = source.AsIList();
			if (list != null) return list[index];
			
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
			// Little short-circuit taken from Enumerable.cs
			var list = source.AsIList();
			if (list != null) {
				if (list.Count > 0)
					return list [0];
				else
					throw new InvalidOperationException("source is empty");
			}
			
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
			// Little short-circuit taken from Enumerable.cs
			var list = source.AsIList();
			if (list != null)
				return (list.Count > 0) ? list [0] : default(TSource);
			
			TSource first;
			int index;
			
			source.IsNotLast();
			bool result = source.GetParallelEnumerator().MoveNext(out first, out index);
			source.IsLast();
			
			return result ? first : default(TSource);
		}
		#endregion
	
		#region
		public static IParallelEnumerable<T> DefaultIfEmpty<T>(this IParallelEnumerable<T> source)
		{
			return source.DefaultIfEmpty(default(T));
		}
		
		public static IParallelEnumerable<T> DefaultIfEmpty<T>(this IParallelEnumerable<T> source, T defValue)
		{
			if (source.Any())
				return source;
			else
				return ParallelEnumerable.Repeat(defValue, 1);
		}
		#endregion
		
		#region
		public static T[] ToArray<T>(this IParallelEnumerable<T> source)
		{
			List<T> temp = source.Aggregate(() => new List<T>(),
			                                      (list, e) => { list.Add(e); return list; },
			                                      (list, list2) => { list.AddRange(list2); return list; },
			                                      (list) => list);
			return temp.ToArray();
			
		}
		#endregion
		
		#region Zip
		public static IParallelEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IParallelEnumerable<TFirst> first,
		                                                                         IEnumerable<TSecond> second,
		                                                                         Func<TFirst, TSecond, TResult> resultSelector)
		{
			throw new NotImplementedException();
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