// ParallelEnumerableHelper.cs
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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Linq
{
	
	internal static class ParallelEnumerableHelper
	{
		internal const int DefaultDop = -1;
		
		#region Internally used operators
		internal static int Dop<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			return temp == null ? DefaultDop : temp.Dop;
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
		
		internal static IParallelEnumerator<T> GetParallelEnumerator<T>(this IParallelEnumerable<T> source,
		                                                                bool isLast)
		{
			ParallelEnumerableBase<T> temp = source as ParallelEnumerableBase<T>;
			return temp.GetParallelEnumerator(isLast);
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
		
		internal static void Process<TSource>(IParallelEnumerable<TSource> source,
		                                    Action<int, TSource> action, bool block)
		{
			Process(source, (i, s) => { action(i, s); return true; }, block);
		}
		
		internal static void Process<TSource>(IParallelEnumerable<TSource> source,
		                                      Func<int, TSource, bool> action, bool block)
		{
			IParallelEnumerator<TSource> feedEnum = source.GetParallelEnumerator(false);
			
			Parallel.SpawnBestNumber(delegate {
				TSource item;
				int i;
				while (feedEnum.MoveNext(out item, out i)) {
					if (!action(i, item))
						break;
				}
			}, source.Dop(), block, null);
		}
		
		internal static IParallelEnumerable<TResult> 
			Process<TSource, TResult>(IParallelEnumerable<TSource> source,
			                          Func<int, TSource, ResultReturn<TResult>> action)
		{			
			Func<IParallelEnumerator<TSource>, ResultReturn<TResult>> wrapper 
			                  = delegate(IParallelEnumerator<TSource> feedEnum) {
				TSource item;
				int i;
				if (feedEnum.MoveNext (out item, out i)) {
					return action (i, item);
				} else {
					return ResultReturn<TResult>.False;
				}
			};
			
			ParallelEnumerableBase<TResult> result =
				ParallelEnumerableFactory.GetFromBlockingCollection<TSource, TResult>(wrapper, source);
			
			if (source.IsOrdered())
				result.IsOrdered = true;
			
			return result;
		}
		#endregion
		
		
		// ------------------------------------------------------------------------------------
		
		
		#region Exception handling
		internal static void AssertSourceNotNull<T>(IParallelEnumerable<T> source)
		{
			AssertNotNull(source, "source");
		}
		
		internal static void AssertNotNull<T>(T obj, string paramName) where T : class
		{
			if (obj == null)
				throw new ArgumentNullException(paramName);
		}
		
		internal static void AssertInRange<T>(T e, Func<T, bool> rangePredicate, string paramName)
		{
			if (rangePredicate(e))
				throw new ArgumentOutOfRangeException(paramName);
		}
		
		internal static void Assert<T, TException>(T obj, Func<T, bool> predicate, Func<TException> exFactory) where TException : Exception
		{
			if (predicate(obj))
				throw exFactory();
		}
		#endregion
		
		#region Helper function / predicate
		internal static bool NullableExistencePredicate<T>(Nullable<T> nullable) where T : struct
		{
			return nullable.HasValue;
		}

		internal static T NullableExtractor<T>(Nullable<T> nullable) where T : struct
		{
			return nullable.Value;
		}

		// For bestSelector, if first arg is better than second returns true
		internal static T BestOrder<T>(IParallelEnumerable<T> source, Func<T, T, bool> bestSelector, T seed)
		{
			T best = seed;
			
			best = source.Aggregate(() => seed,
			                        (first, second) => (bestSelector(first, second)) ? first : second,
			                        (first, second) => (bestSelector(first, second)) ? first : second,
			                        (e) => e);
			return best;
		}
		#endregion
	}
}
