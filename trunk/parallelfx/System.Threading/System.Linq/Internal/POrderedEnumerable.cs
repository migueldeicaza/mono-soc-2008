// POrderedEnumerable.cs
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
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	internal class POrderedEnumerable<T>: ParallelEnumerableBase<T>, IParallelOrderedEnumerable<T>
	{
		IParallelEnumerable<T> source;
		bool doSort = true;

		Comparison<T> comparison;
		
		internal POrderedEnumerable(IParallelEnumerable<T> source, Comparison<T> comparison)
			: base (source.Dop())
		{
			this.source = source;

			this.comparison = comparison;
			// OrderBy explicitely turn on ordering
			this.SetOrdered();
		}
		
		public IParallelOrderedEnumerable<T> CreateParallelOrderedEnumerable<TKey>(Func<T, TKey> keySelector,
		                                                                           IComparer<TKey> comparer,
		                                                                           bool descending)
		{
			this.doSort = false;
			
			return new POrderedEnumerable<T> (this, FromComparer(comparison, keySelector,
			                                                     comparer, descending));
		}
		
		public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector,
		                                                           IComparer<TKey> comparer,
		                                                           bool descending)
		{
			return CreateParallelOrderedEnumerable(keySelector, comparer, descending);
		}
		
		static Comparison<T> FromComparer<TKey>(Comparison<T> parent, Func<T, TKey> ks,
		                                        IComparer<TKey> comparer, bool descending)
		{
			Comparison<T> child;
			if (!descending)
				child = (e1, e2) =>
					comparer.Compare(ks(e1), ks(e2));
			else 
				child = (e1, e2) => 
					comparer.Compare(ks(e2), ks(e1));
			
			if (parent != null) {
				return (e1, e2) => {
					int result = parent(e1, e2);
					if (result == 0)
						result = child(e1, e2);
					
					return result;
				};
			} else {
				return child;
			}
		}
		
		public override IParallelEnumerator<T> GetParallelEnumerator(bool isLast)
		{
			if (doSort) {
				IParallelEnumerable<T> enumerable = SortSource();
				return enumerable.GetParallelEnumerator(isLast);
			} else {
				return source.GetParallelEnumerator(isLast);
			}
		}
		
		IParallelEnumerable<T> SortSource()
		{
			List<T> temp = source.ToList();
			temp.Sort(comparison);
			return ParallelEnumerableFactory.GetFromIEnumerable(temp, source.Dop());
		}
	}
}
