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
		static int Dop<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerable<T> temp = source as ParallelEnumerable<T>;
			return temp == null ? -1 : temp.Dop;
		}
		
		static ParallelEnumerable<TResult> Process<TSource, TResult>(IParallelEnumerable<TSource> source,
		                                                    Action<BlockingCollection<TResult>, int, TSource> action)
		{
			BlockingCollection<TResult> resultBuffer = new BlockingCollection<TResult>();
			IEnumerator<TSource> feedEnum = source.GetEnumerator();
			int index = -1;
			
			Parallel.SpawnBestNumber(delegate {
				// Technically it's not exact, we might get preempted
				int i = Interlocked.Increment(ref index);
				while (feedEnum.MoveNext()) {
					// TODO: need to throw possible Exception stored in Task when the processing is finished
					action(resultBuffer, i, feedEnum.Current);
				}
				resultBuffer.CompleteAdding();
			}, source.Dop());
			
			return new ParallelEnumerable<TResult>(resultBuffer, source.Dop());
		}
		
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, TResult> selector)
		{
			return Select(source, (TSource e, int index) => selector(e));
		}
		
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, int, TResult> selector)
		{
			return Process<TSource, TResult>(source, delegate (BlockingCollection<TResult> resultBuffer, int i, TSource e) {
				resultBuffer.Add(selector(e, i));
			});
		}
		
		public static IParallelEnumerable<TSource> Where<TSource>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, bool> predicate)
		{
			return Where(source, (TSource e, int index) => predicate(e));
		}
		
		public static IParallelEnumerable<TSource> Where<TSource>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, int, bool> predicate)
		{
			return Process<TSource, TSource>(source, delegate (BlockingCollection<TSource> resultBuffer, int i, TSource e) {
				if (predicate(e, i))
					resultBuffer.Add(e);
			});
		}
	}
}
