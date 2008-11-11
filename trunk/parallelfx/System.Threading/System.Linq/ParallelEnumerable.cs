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
		}
		
		internal static void IsLast<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			if (temp == null)
				return;
			temp.IsLast = true;
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
		
		
		// ------------------------------------------------------------------------------------
		
		
		#region Internal link to SpawnBestNumber
		
		static void Process<TSource>(IParallelEnumerable<TSource> source, Action<int, TSource> action, bool block)
		{
			Process(source, (i, s) => { action(i, s); return true; }, block);
		}
		
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
		#endregion
		
		
		// ------------------------------------------------------------------------------------
		
		
		#region Exception handling
		static void AssertSourceNotNull<T>(IParallelEnumerable<T> source)
		{
			AssertNotNull(source, "source");
		}
		
		static void AssertNotNull<T>(T obj, string paramName) where T : class
		{
			if (obj == null)
				throw new ArgumentNullException(paramName);
		}
		
		static void AssertInRange<T>(T e, Func<T, bool> rangePredicate, string paramName)
		{
			if (rangePredicate(e))
				throw new ArgumentOutOfRangeException(paramName);
		}
		
		static void Assert<T, TException>(T obj, Func<T, bool> predicate, Func<TException> exFactory) where TException : Exception
		{
			if (predicate(obj))
				throw exFactory();
		}
		#endregion
		
		#region Helper function / predicate
		static bool NullableExistencePredicate<T>(T? nullable) where T : struct
		{
			return nullable.HasValue;
		}

		static T NullableExtractor<T>(T? nullable) where T : struct
		{
			return nullable.Value;
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
		
		// ------------------------------------------------------------------------------------
		
		
		
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
		
		#region
		public static IParallelEnumerable<T> Reverse<T>(this IParallelEnumerable<T> source)
		{
			// HACK
			List<T> temp = source.ToList();
			temp.Reverse();
			return ParallelEnumerableFactory.GetFromIEnumerable(temp, source.Dop());
		}
		#endregion
		
		#region OrderBy
		public static IParallelOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                         Func<TSource, TKey> keySelector)
		{
			return source.OrderBy(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                         Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			if (keySelector == null)
				throw new ArgumentNullException("keySelector");
			
			return OrderByInternal<TSource>(source, (e1, e2) => {
				if (e1 == null || e2 == null)
					return 0;
				return comparer.Compare(keySelector(e1), keySelector(e2));
			});
		}
		
		public static IParallelOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                                   Func<TSource, TKey> keySelector)
		{
			return source.OrderByDescending(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IParallelEnumerable<TSource> source,
		                                                                  Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			return OrderByInternal<TSource>(source, (e1, e2) => comparer.Compare(keySelector(e2), keySelector(e1)));
		}
		
		static IParallelOrderedEnumerable<TSource> OrderByInternal<TSource>(IParallelEnumerable<TSource> source,
		                                                           System.Comparison<TSource> comparison)
		{
			return ParallelEnumerableFactory.GetOrdered(source, comparison);
		}
		#endregion
		
		#region ThenBy
		public static IParallelOrderedEnumerable<T> ThenBy<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                             Func<T, TKey> keySelector)
		{
			return source.ThenBy(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<T> ThenBy<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                             Func<T, TKey> keySelector,
		                                                             IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			
			return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, false);
		}
		
		public static IParallelOrderedEnumerable<T> ThenByDescending<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                                       Func<T, TKey> keySelector)
		{
			return source.ThenByDescending(keySelector, Comparer<TKey>.Default);
		}
		
		public static IParallelOrderedEnumerable<T> ThenByDescending<T, TKey> (this IParallelOrderedEnumerable<T> source,
		                                                                       Func<T, TKey> keySelector,
		                                                                       IComparer<TKey> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException("comparer");
			
			return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, true);
		}
		#endregion 
		
		#region Average
		public static double Average(this IParallelEnumerable<int> source)
		{
			return source.Aggregate(() => new int[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / ((double)acc[1]));
		}
		
		public static double Average(this IParallelEnumerable<long> source)
		{
			return source.Aggregate(() => new long[2],
			                        (acc, e) => { acc[0] += e; acc[1]++; return acc; },
			                        (acc1, acc2) => { acc1[0] += acc2[0]; acc1[1] += acc2[1]; return acc1; },
			                        (acc) => acc[0] / ((double)acc[1]));
		}
		#endregion
		
		#region Sum
		public static int Sum(IParallelEnumerable<int> source)
		{
			return source.Aggregate(0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static byte Sum(IParallelEnumerable<byte> source)
		{
			return source.Aggregate((byte)0, (e1, e2) => (byte)(e1 + e2), (sum1, sum2) => (byte)(sum1 + sum2), (sum) => sum);
		}
		
		public static short Sum(IParallelEnumerable<short> source)
		{
			return source.Aggregate((short)0, (e1, e2) => (short)(e1 + e2), (sum1, sum2) => (short)(sum1 + sum2), (sum) => sum);
		}
		
		public static long Sum(IParallelEnumerable<long> source)
		{
			return source.Aggregate((long)0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static float Sum(IParallelEnumerable<float> source)
		{
			return source.Aggregate(0.0f, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static double Sum(IParallelEnumerable<double> source)
		{
			return source.Aggregate(0.0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		
		public static decimal Sum(IParallelEnumerable<decimal> source)
		{
			return source.Aggregate((decimal)0, (e1, e2) => e1 + e2, (sum1, sum2) => sum1 + sum2, (sum) => sum);
		}
		#endregion

		#region Sum (nullable)
		public static int Sum(IParallelEnumerable<int?> source)
		{
			return source.Where(NullableExistencePredicate<int>).Select<int?, int>(NullableExtractor<int>).Sum();
		}
		
		public static byte Sum(IParallelEnumerable<byte?> source)
		{
			return Sum(source.Where(NullableExistencePredicate<byte>).Select<byte?, byte>(NullableExtractor<byte>));
		}
		
		/*public static short Sum(IParallelEnumerable<short?> source)
		{
			return source.Where(NullableExistencePredicate<short>).Select<int?, int>(NullableExtractor<short>).Sum();
		}*/
		
		public static long Sum(IParallelEnumerable<long?> source)
		{
			return source.Where(NullableExistencePredicate<long>).Select<long?, long>(NullableExtractor<long>).Sum();
		}
		
		public static float Sum(IParallelEnumerable<float?> source)
		{
			return source.Where(NullableExistencePredicate<float>).Select<float?, float>(NullableExtractor<float>).Sum();
		}
		
		public static double Sum(IParallelEnumerable<double?> source)
		{
			return source.Where(NullableExistencePredicate<double>).Select<double?, double>(NullableExtractor<double>).Sum();
		}
		
		public static decimal Sum(IParallelEnumerable<decimal?> source)
		{
			return source.Where(NullableExistencePredicate<decimal>).Select<decimal?, decimal>(NullableExtractor<decimal>).Sum();
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
		#endregion

		
		#region Min - Max (Nullable)
		
		/*public static int Min(this IParallelEnumerable<int?> source)
		{
			return source.Where(NullableExistencePredicate<int>).Select(NullableExtractor<int>).Min();
		}
		
		public static byte Min(this IParallelEnumerable<byte?> source)
		{
			return source.Where(NullableExistencePredicate<byte>).Min();
		}
		
		public static short Min(this IParallelEnumerable<short?> source)
		{
			return source.Where(NullableExistencePredicate<short>).Min();
		}
		
		public static long Min(this IParallelEnumerable<long?> source)
		{
			return source.Where(NullableExistencePredicate<long>).Min();
		}
		
		public static float Min(this IParallelEnumerable<float?> source)
		{
			return source.Where(NullableExistencePredicate<float>).Min();
		}
		
		public static double Min(this IParallelEnumerable<double?> source)
		{
			return source.Where(NullableExistencePredicate<double>).Min();
		}
		
		public static decimal Min(this IParallelEnumerable<decimal?> source)
		{
			return source.Where(NullableExistencePredicate<decimal>).Min();
		}
		
		public static int Max(this IParallelEnumerable<int?> source)
		{
			return source.Where(NullableExistencePredicate<int>).Max();
		}
		
		public static byte Max(this IParallelEnumerable<byte?> source)
		{
			return source.Where(NullableExistencePredicate<byte>).Max();
		}
		
		public static short Max(this IParallelEnumerable<short?> source)
		{
			return source.Where(NullableExistencePredicate<short>).Max();
		}
		
		public static long Max(this IParallelEnumerable<long?> source)
		{
			return source.Where(NullableExistencePredicate<long>).Max();
		}
		
		public static float Max(this IParallelEnumerable<float?> source)
		{
			return source.Where(NullableExistencePredicate<float>).Max();
		}
		
		public static double Max(this IParallelEnumerable<double?> source)
		{
			return source.Where(NullableExistencePredicate<double>).Max();
		}
		
		public static decimal Max(this IParallelEnumerable<decimal?> source)
		{
			return source.Where(NullableExistencePredicate<decimal>).Max();
		}*/
		#endregion
		
		#region Aggregate
		public static TResult Aggregate<TSource, TAccumulate, TResult>(this IParallelEnumerable<TSource> source,
		                                                               TAccumulate seed,
		                                                               Func<TAccumulate, TSource, TAccumulate> intermediateReduceFunc,
		                                                               Func<TAccumulate, TAccumulate, TAccumulate> finalReduceFunc,
		                                                               Func<TAccumulate, TResult> resultSelector)
		{
			return source.Aggregate(() => seed, intermediateReduceFunc, finalReduceFunc, resultSelector);
		}
		
		public static TResult Aggregate<TSource, TAccumulate, TResult>(this IParallelEnumerable<TSource> source,
		                                                               Func<TAccumulate> seedFactory,
		                                                               Func<TAccumulate, TSource, TAccumulate> intermediateReduceFunc,
		                                                               Func<TAccumulate, TAccumulate, TAccumulate> finalReduceFunc,
		                                                               Func<TAccumulate, TResult> resultSelector)
		{
			int count = Parallel.GetBestWorkerNumber();
			
			TAccumulate[] accumulators = new TAccumulate[count];
			int[] switches = new int[count];
			
			for (int i = 0; i < count; i++) {
				accumulators[i] = seedFactory();
			}
			
			int index = -1;
			
			// Process to intermediate result into distinct domain
			Process<TSource>(source, delegate (int j, TSource element) {
				int i;
				
				// HACK: do the local var thing on Process/SpawnBestNumber side
				do {
					i = Interlocked.Increment(ref index) % count;
				} while (Interlocked.Exchange(ref switches[i], 1) == 1);
				// Reduce results on each domain
				accumulators[i] = intermediateReduceFunc(accumulators[i], element);
				switches[i] = 0;
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
		
		public static IParallelEnumerable<TSource> TakeWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, bool> predicate)
		{
			return source.SkipWhile((e, i) => predicate(e));
		}
		
		public static IParallelEnumerable<TSource> TakeWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, int, bool> predicate)
		{
			bool stopFlag = true;
			
			return Process<TSource, TSource> (source, delegate (Action<TSource, bool, int> adder, int i, TSource e) {
				if (stopFlag && (stopFlag = predicate(e, i))) {
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
		
		public static IParallelEnumerable<TSource> SkipWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, bool> predicate)
		{
			return source.SkipWhile((e, i) => predicate(e));
		}
		
		public static IParallelEnumerable<TSource> SkipWhile<TSource>(this IParallelEnumerable<TSource> source, 
		                                                              Func<TSource, int, bool> predicate)
		{
			bool predicateStatus = true;
			
			return source.Where((element, index) => {
				if (!predicateStatus)
					return true;
				
				bool result = predicate(element, index);
				if (!result)
					predicateStatus = true;
				return !result;
			});
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
			AssertSourceNotNull(source);
			
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
		
		public static TSource First<TSource>(this IParallelEnumerable<TSource> source,
		                                     Func<TSource, bool> predicate)
		{
			AssertSourceNotNull(source);
			
			TSource element = default(TSource);
			
			bool result = false;
			
			Process<TSource>(source, delegate (int i, TSource e) {
				if (predicate(e)) {
					element = e;
					result = true;
					return false;
				} else {
					return true;
				}
			}, true);
			
			Assert(result, (r) => !r, () => new Exception("No element found"));
			
			return element;
		}
		
		public static TSource FirstOrDefault<TSource>(this IParallelEnumerable<TSource> source)
		{
			AssertSourceNotNull(source);
			
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
		
		public static TSource FirstOrDefault<TSource>(this IParallelEnumerable<TSource> source,
		                                              Func<TSource, bool> predicate)
		{
			AssertSourceNotNull(source);
			
			TSource element = default(TSource);
			
			Process<TSource>(source, delegate (int i, TSource e) {
				if (predicate(e)) {
					element = e;
					return false;
				} else {
					return true;
				}
			}, true);
			
			return element;
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
		
		#region ToArray - ToList
		public static List<T> ToList<T>(this IParallelEnumerable<T> source)
		{
			List<T> temp = source.Aggregate(() => new List<T>(),
			                                (list, e) => { list.Add(e); return list; },
			                                (list, list2) => { list.AddRange(list2); return list; },
			                                (list) => list);
			return temp;
		}
		
		public static T[] ToArray<T>(this IParallelEnumerable<T> source)
		{
			return source.ToList().ToArray();	
		}
		#endregion
		
		#region Zip
		public static IParallelEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IParallelEnumerable<TFirst> first,
		                                                                         IEnumerable<TSecond> second,
		                                                                         Func<TFirst, TSecond, TResult> resultSelector)
		{
			IConcurrentCollection<TResult> result = new ConcurrentStack<TResult>();
			
			IParallelEnumerator<TFirst> pEnum1 = first.GetParallelEnumerator();

			IParallelEnumerable<TSecond> pEnumerable = 
				second as IParallelEnumerable<TSecond> ?? ParallelEnumerableFactory.GetFromIEnumerable(second, first.Dop());
			IParallelEnumerator<TSecond> pEnum2 = pEnumerable.GetParallelEnumerator();

			TFirst element1;
			TSecond element2;
			int index;

			while (pEnum1.MoveNext(out element1, out index) && pEnum2.MoveNext(out element2, out index)) {
				//Console.WriteLine("({0},{1})", element1.ToString(), element2.ToString());
				result.Add(resultSelector(element1, element2));
			}

			return ParallelEnumerableFactory.GetFromIEnumerable(result, first.Dop());
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
