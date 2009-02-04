// ParallelQuery.cs
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

namespace System.Linq
{
	// TODO: keeping state about isOrdered and isLast is bad because a part of the query may be used directly
	// and then chained back to another so isLast is false in this case
	public static class ParallelQuery
	{
		public static IParallelEnumerable<T> AsParallel<T>(this IEnumerable<T> source)
		{
			return source.AsParallel(ParallelEnumerableHelper.DefaultDop);
		}
		
		public static IParallelEnumerable<T> AsParallel<T>(this IEnumerable<T> source, int dop)
		{
			IParallelEnumerable<T> temp = source as IParallelEnumerable<T>;
			// No need to convert
			if (temp != null)
				return temp;
			
			return ParallelEnumerableFactory.GetFromIEnumerable<T>(source, dop);
		}
		
		public static IParallelEnumerable<T> AsOrdered<T>(this IParallelEnumerable<T> source)
		{
			source.SetOrdered();
			return source;
		}
		
		public static IParallelEnumerable<T> AsUnordered<T>(this IParallelEnumerable<T> source)
		{
			// Add some logic that check if the ParallelEnumerable is a OrderedParallelEnumerable
			// and thus that no other action occured after a OrderBy/ThenBy to remove that part
			// of the query since it will be suffled anyway
			source.SetUnordered();
			return source;
		}
		
		public static IEnumerable<T> AsSequential<T>(this IParallelEnumerable<T> source)
		{
			return (IEnumerable<T>)source;
		}
	}
}
