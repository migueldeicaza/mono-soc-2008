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

namespace System.Threading.Linq
{
	public static class ParallelEnumerable
	{
		static int Dop<T>(this IParallelEnumerable<T> source)
		{
			ParallelEnumerable<T> temp = source as ParallelEnumerable<T>;
			return temp == null ? -1 : temp.Dop;
		}
		
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, TResult> selector)
		{
			return Select(source, (TSource e, int index) => selector(e));
		}
		
		public static IParallelEnumerable<TResult> Select<TSource, TResult>(this IParallelEnumerable<TSource> source,
		                                                                    Func<TSource, int, TResult> selector)
		{
			BlockingCollection<TResult> resultBuffer = new BlockingCollection<TResult>();
			IEnumerator<TSource> feedEnum = source.GetEnumerator();
			int index = -1;
			
			//TODO: Consider using an Enumerator inside the anonymous method
			Parallel.SpawnBestNumber(delegate {
				// Technically it's not exact, we might get preempted
				int i = Interlocked.Increment(ref index);
				while (feedEnum.MoveNext()) {
					// TODO: need to throw possible Exception stored in Task when the processing is finished
					resultBuffer.Add(selector(feedEnum.Current, i));
				}
				resultBuffer.CompleteAdding();
			}, source.Dop());
			
			return new ParallelEnumerable<TResult>(resultBuffer, source.Dop());
		}
	}
}
