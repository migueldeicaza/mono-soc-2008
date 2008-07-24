// ParallelEnumerableFactory.cs
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
using System.Threading.Collections;

namespace System.Linq
{
	
	internal static class ParallelEnumerableFactory
	{
		public static IParallelEnumerable<T> GetFromBlockingCollection<TSource, T>(Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action,
		                                                                           IParallelEnumerable<TSource> source)
		{
			return new System.Linq.PEBlockingCollection<TSource, T>(new BlockingCollection<T>(), action, source, source.Dop());
		}
		
		public static IParallelEnumerable<T> GetFromBlockingCollection<TSource, T>(BlockingCollection<T> coll,
		                                                                  Func<IParallelEnumerator<TSource>, Action<T, bool, int>, bool> action,
		                                                                  IParallelEnumerable<TSource> source)
		{
			return new System.Linq.PEBlockingCollection<TSource, T>(coll, action, source, source.Dop());
		}
		
		public static IParallelEnumerable<T> GetFromIEnumerable<T>(IEnumerable<T> coll, int dop)
		{
			return new System.Linq.PEIEnumerable<T>(coll, dop);
		}
		
		public static IParallelEnumerable<T> GetFromIParallelEnumerable<T>(int dop, params IParallelEnumerable<T>[] enumerables)
		{
			return new System.Linq.PEConcat<T>(enumerables, dop);
		}
		
		public static IParallelEnumerable<int> GetFromRange(int start, int count, int dop)
		{
			return new PERange(start, count, dop);
		}
		
		public static IParallelEnumerable<T> GetFromRepeat<T>(T element, int count, int dop)
		{
			return new PERepeat<T>(element, count, dop);
		}
	}
}
