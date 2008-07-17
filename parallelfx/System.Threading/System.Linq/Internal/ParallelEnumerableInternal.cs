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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Collections;

namespace System.Threading.Linq
{
	internal class ParallelEnumerable<T>: IParallelEnumerable<T>
	{
		Func<IEnumerator<T>> getEnumerator;
		// Dop is Degree of Parallelism. It corresponds to the ideal number of threads
		// that should be used to compute the query.
		int dop;
		
		// For ctor 2
		BlockingCollection<T> bColl;
		// For ctor 3
		int from, to, step;
		// For ctor 1
		IEnumerable<T> uEnumerable;
		IEnumerator<T> uEnumerator;
		SpinLock sl = new SpinLock(false);
		
		// Will use chunk partitionning, used by AsParallel()
		public ParallelEnumerable(IEnumerable<T> uEnumerable, int dop): this(dop)
		{
			this.uEnumerable = uEnumerable;
			this.uEnumerator = uEnumerable.GetEnumerator();
			getEnumerator = GetEnumeratorFromStandardIEnumerable;
		}
		
		IEnumerator<T> GetEnumeratorFromStandardIEnumerable()
		{
			const int chunkSize = 10;
			bool cont = true;
			int i;
			
			while (cont) {
				T[] buffer = new T[chunkSize];
				
				try {
					sl.Enter();
					for (i = 0; i < chunkSize; i++) {
						if (!uEnumerator.MoveNext()) {
							cont = false;
							break;
						}
						buffer[i] = uEnumerator.Current;
					}
				} finally {
					sl.Exit();
				}
				
				for (int j = 0; j < i; j++)
					yield return buffer[j];
			}
		}
		
		// Comes from the return of ParallelEnumerable extension method
		public ParallelEnumerable(BlockingCollection<T> bColl, int dop): this(dop)
		{
			this.bColl = bColl;
			getEnumerator = GetEnumeratorFromBlockingCollection;
		}
				
		IEnumerator<T> GetEnumeratorFromBlockingCollection()
		{
			while (!bColl.IsCompleted) {
				foreach (T item in bColl.GetConsumingEnumerable())
					yield return item;
			}
		}
		
		// Will use range partitionning
		public ParallelEnumerable(int dop)
		{
			this.dop = dop;
		}
		
		public static ParallelEnumerable<int> GetRangeParallelEnumerable(int from, int to, int step, int dop)
		{
			//HACK: rather ugly
			ParallelEnumerable<int> temp = new ParallelEnumerable<int>(dop);
			temp.from = from;
			temp.to = to;
			temp.step = step;
			
			temp.getEnumerator = temp.GetEnumeratorFromRange;
			
			return temp;
		}
		
		IEnumerator<int> GetEnumeratorFromRange()
		{
			int counter = from;
			do {
				int value = Interlocked.Add(ref counter, step) - step;
				yield return value;
			} while (counter < to);
			
		}
		
		public IEnumerator<T> GetEnumerator(bool enablePipelining)
		{
			// Don't care about Pipelining for the moment
			// Just a matter of calling Task.WaitAll in the correct place
			return getEnumerator();
		}
		
		public int Dop {
			get {
				return dop;
			}
		}
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return getEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)getEnumerator();
		}
	}
}
